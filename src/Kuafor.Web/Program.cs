using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Kuafor.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// GEÇİCİ: Tüm provider'ları temizle, sadece basit console kullan
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.SingleLine = true; });

// GEÇİCİ: ASP.NET Core'un "unhandled exception" logunu sustur
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics", LogLevel.None);

// TODO[DB]: SQLite DbContext'i burada ekle
// builder.Services.AddDbContext<AppDbContext>(o =>
//     o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// TODO[Identity]: Identity kurulumunu burada yap
// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// TODO[Auth]: Kimlik doğrulama/Yetkilendirme middlewares
// app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Areas için genel route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// Varsayılan kök
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
