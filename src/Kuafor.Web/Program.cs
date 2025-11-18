using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Services;
using Kuafor.Web.Services.BackgroundServices;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Controllers.Api.V1;
using Kuafor.Web.Areas.Admin.Controllers;
using OfficeOpenXml;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// 1. Veritabanı ve Identity'yi yapılandır
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.CommandTimeout(120); // Azure'daki ilk veritabanı oluşturma işlemi için zaman aşımını artır
    }));

// >>> Identity politikalarıyla birlikte ve token providers dahil <<<
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 6;
    opts.Password.RequireDigit = true;
    opts.User.RequireUniqueEmail = true;

    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Phone provider dahil

// 2. Diğer servisleri ekle
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Bellek önbelleği (SMS reset oturumları vs. için)
builder.Services.AddMemoryCache();

// ... (Tüm servis kayıtların burada) ...
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IStylistService, StylistService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITemplateService, RazorTemplateService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdisyonService, AdisyonService>();
builder.Services.AddScoped<IPackageService, PackageManagementService>();
builder.Services.AddScoped<IStylistWorkingHoursService, StylistWorkingHoursService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICustomerAnalyticsService, CustomerAnalyticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
builder.Services.AddScoped<IMarketingService, MarketingService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ~~ Eski satır: builder.Services.AddScoped<ISmsService, SmsService>();
// >>> Twilio'ya yönlendirildi <<<
builder.Services.AddScoped<ISmsService, TwilioSmsSender>();

builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IWhatsAppTemplateService, WhatsAppTemplateService>();
builder.Services.AddScoped<IWhatsAppMediaService, WhatsAppMediaService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();

// API Controller'ları için
builder.Services.AddScoped<Kuafor.Web.Controllers.Api.V1.ExportController>();

// Admin Area Controller'ları için 
builder.Services.AddScoped<FinancialController>();
builder.Services.AddScoped<Kuafor.Web.Areas.Admin.Controllers.ExportController>();

// ÖNEMLİ: Yarış durumunu engellemek için bu servis şimdilik kapalı kalacak.
// builder.Services.AddHostedService<AppointmentReminderService>();

// 3. Authentication (Kimlik Doğrulama) Ayarları
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(jwtSecret))
{
    // Azure'da bu ayar "JwtSettings__SecretKey" olarak aranacak.
    jwtSecret = builder.Configuration["JwtSettings__SecretKey"];
    if (string.IsNullOrEmpty(jwtSecret))
    {
        throw new InvalidOperationException("JWT Secret Key is required! Check Azure App Settings for 'JwtSettings__SecretKey'.");
    }
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    // Web sayfaları için varsayılan kimlik doğrulama yöntemi Cookie'dir.
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Bu ayarları daha sonra açabilirsin
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Authorization (Yetkilendirme)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    // Diğer roller...
});

var app = builder.Build();

// ===== 5. KONTROLLÜ VERİTABANI OLUŞTURMA VE BAŞLATMA =====
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Azure App Service başlatılıyor...");
        logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
        logger.LogInformation($"Connection String: {(string.IsNullOrEmpty(connectionString) ? "BULUNAMADI!" : "AYARLANDI")}");

        logger.LogInformation("Veritabanı migration'ları uygulanıyor...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migration'lar tamamlandı.");

        logger.LogInformation("DbInitializer çalıştırılıyor...");
        await DbInitializer.InitializeAsync(services);
        logger.LogInformation("DbInitializer tamamlandı.");

        logger.LogInformation("Azure App Service başarıyla başlatıldı!");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Uygulama başlangıcında veritabanı kurulumu sırasında kritik bir hata oluştu.");
    logger.LogError($"Hata detayı: {ex.Message}");
    logger.LogError($"Stack trace: {ex.StackTrace}");

    // Azure'da daha iyi hata raporlama için
    if (ex.InnerException != null)
    {
        logger.LogError($"Inner exception: {ex.InnerException.Message}");
    }
}
// =======================================================

// 6. HTTP Pipeline Yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
