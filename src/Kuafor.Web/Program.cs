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
using Kuafor.Web.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// ===== DEĞİŞİKLİK BURADA BAŞLIYOR =====
// Azure App Ayarlarını ve appsettings.json'ı okumak için standart .NET yapılandırması.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// ===== DEĞİŞİKLİK BURADA BİTİYOR =====


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
var jwtSecret = jwtSettings["SecretKey"];

// JWT Secret Key validation
if (string.IsNullOrEmpty(jwtSecret) || jwtSecret == "REPLACE_WITH_USER_SECRETS_OR_ENVIRONMENT_VARIABLE")
{
    throw new InvalidOperationException(
        "JWT Secret Key is required! " +
        "Set it using: dotnet user-secrets set \"JwtSettings:SecretKey\" \"your-secret-key\" " +
        "or set JWT_SECRET_KEY environment variable.");
}

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long for security!");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "KuaforApp",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "KuaforUsers",
        ClockSkew = TimeSpan.Zero
    };
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "http://localhost:5001")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .AllowCredentials();
    });
    
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .AllowCredentials();
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
builder.Services.AddHostedService<AppointmentReminderService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StylistOnly", policy => policy.RequireRole("Stylist"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Stylist"));
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
// CORS Policy'yi environment'a göre seç
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

// Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
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