using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Kuafor.Web.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Services;
using Kuafor.Web.Services.BackgroundServices;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DotNetEnv;
using Kuafor.Web.Models.Entities;


var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
Console.WriteLine($"Looking for .env file at: {envPath}");
Console.WriteLine($"File exists: {File.Exists(envPath)}");

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
    Console.WriteLine(".env file loaded successfully!");
}
else
{
    Console.WriteLine(".env file not found!");
}

var builder = WebApplication.CreateBuilder(args);

// Environment variables'ları configuration'a ekle
builder.Configuration.AddEnvironmentVariables();

// Database bağlantısı
var dbServer = Env.GetString("DB_SERVER");
var dbDatabase = Env.GetString("DB_DATABASE");
var dbUserId = Env.GetString("DB_USER_ID");
var dbPassword = Env.GetString("DB_PASSWORD");

// Debug: Environment variables kontrol et
Console.WriteLine($"DB_SERVER: {dbServer}");
Console.WriteLine($"DB_DATABASE: {dbDatabase}");
Console.WriteLine($"DB_USER_ID: {dbUserId}");
Console.WriteLine($"DB_PASSWORD: {dbPassword}");

var connectionString = $"Server={dbServer};" +
                      $"Database={dbDatabase};" +
                      $"User Id={dbUserId};" +
                      $"Password={dbPassword};" +
                      $"MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// Database developer page exception filter removed for .NET 9 compatibility

// Identity configuration for .NET 9
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();

// Razor Pages ekle - .NET 9 için gerekli
builder.Services.AddRazorPages();
builder.Services.AddScoped<IValidationService, ValidationService>();

// API ve Swagger kurulumu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Kuafor API", 
        Version = "v1",
        Description = "Kuafor randevu sistemi API'si"
    });
    
    // JWT Authentication için Swagger UI'da Authorization butonu
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Cookie Authentication (Web sayfaları için)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.ReturnUrlParameter = "returnUrl";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 302;
        context.Response.Headers["Location"] = context.RedirectUri;
        return Task.CompletedTask;
    };
});

// JWT Authentication (API için)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Service registrations - Dependency Injection için
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IStylistService, StylistService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

// Yeni eklenen servisler
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITemplateService, RazorTemplateService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdisyonService, AdisyonService>();
builder.Services.AddScoped<IPackageService, PackageManagementService>();
builder.Services.AddScoped<IStylistWorkingHoursService, StylistWorkingHoursService>();

// Yeni stok yönetimi servisi
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Yeni müşteri analitikleri servisi
builder.Services.AddScoped<ICustomerAnalyticsService, CustomerAnalyticsService>();

// Yeni eklenen servisler
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
builder.Services.AddScoped<IMarketingService, MarketingService>();

// Background service
builder.Services.AddHostedService<AppointmentReminderService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StylistOnly", policy => policy.RequireRole("Stylist"));
    
    // Admin ve Stylist erişebilir
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Stylist"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // UseMigrationsEndPoint removed for .NET 9 compatibility
    // Swagger UI'ı sadece development'ta göster
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kuafor API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CORS middleware
app.UseCors("AllowAll");

// Authentication ve Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Areas için genel route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Varsayılan kök
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Access Denied sayfası için
app.MapControllerRoute(
    name: "access-denied",
    pattern: "access-denied",
    defaults: new { controller = "Auth", action = "AccessDenied" });

// DbInitializer'ı aktif hale getir
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();
