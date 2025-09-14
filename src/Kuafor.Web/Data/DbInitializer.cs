using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;

namespace Kuafor.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Rolleri oluştur
        await EnsureRolesAsync(roleManager);
        
        // Admin kullanıcı oluştur
        await EnsureAdminUserAsync(userManager);
        
        // Temel verileri oluştur
        await EnsureBasicDataAsync(context);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Customer", "Stylist" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        var adminEmail = "admin@kuafor.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    private static async Task EnsureBasicDataAsync(ApplicationDbContext context)
    {
        // Şubeler
        if (!await context.Branches.AnyAsync())
        {
            var branches = new[]
            {
                new Branch
                {
                    Name = "Merkez Şube",
                    Address = "Merkez Mahallesi, Kuaför Caddesi No:1",
                    Phone = "0212 555 0001",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    Name = "Monako Şubesi",
                    Address = "Monako Caddesi No:15",
                    Phone = "0212 555 0002",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            await context.Branches.AddRangeAsync(branches);
            await context.SaveChangesAsync();
        }

        // Hizmetler - Sadece ilk kurulumda ekle, sonradan silinen hizmetleri tekrar ekleme
        if (!await context.Services.AnyAsync())
        {
            var services = new[]
            {
                new Service
                {
                    Name = "Saç Kesimi",
                    Description = "Profesyonel saç kesimi hizmeti",
                    DetailedDescription = "Deneyimli kuaförlerimiz tarafından modern tekniklerle saç kesimi",
                    Price = 80.00m,
                    DurationMin = 30,
                    Category = "haircut",
                    IconClass = "bi bi-scissors",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "BOYA BADANA",
                    Description = "Saç boyama ve renklendirme hizmeti",
                    DetailedDescription = "Kaliteli boyalarla saçınızı istediğiniz renge boyuyoruz",
                    Price = 120.00m,
                    DurationMin = 60,
                    Category = "coloring",
                    IconClass = "bi bi-palette",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "Sakal Traşı",
                    Description = "Geleneksel ustura ile sakal traşı",
                    DetailedDescription = "Geleneksel ustura tekniği ile hassas sakal traşı",
                    Price = 50.00m,
                    DurationMin = 20,
                    Category = "beard",
                    IconClass = "bi bi-razor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Service
                {
                    Name = "Saç Yıkama & Kurutma",
                    Description = "Saç yıkama ve şekillendirme",
                    DetailedDescription = "Kaliteli şampuanlarla saç yıkama ve profesyonel kurutma",
                    Price = 40.00m,
                    DurationMin = 25,
                    Category = "care",
                    IconClass = "bi bi-droplet",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        // Kuaförler
        if (!await context.Stylists.AnyAsync())
        {
            var branch = await context.Branches.FirstAsync();
            var stylists = new[]
            {
                new Stylist
                {
                    FirstName = "Ahmet",
                    LastName = "Özdoğan",
                    Email = "ahmet@kuafor.com",
                    Phone = "0532 555 0001",
                    Bio = "10 yıllık deneyimli kuaför",
                    Rating = 4.8m,
                    BranchId = branch.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Stylist
                {
                    FirstName = "Mehmet",
                    LastName = "Yılmaz",
                    Email = "mehmet@kuafor.com",
                    Phone = "0532 555 0002",
                    Bio = "Saç boyama uzmanı",
                    Rating = 4.6m,
                    BranchId = branch.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            await context.Stylists.AddRangeAsync(stylists);
            await context.SaveChangesAsync();
        }
        
        // Kuponlar - Sadece ilk kurulumda ekle
        if (!await context.Coupons.AnyAsync())
        {
            var coupons = new[]
            {
                new Coupon
                {
                    Code = "WELCOME10",
                    Title = "Hoş Geldin İndirimi",
                    DiscountType = "Percent",
                    Amount = 10,
                    MinSpend = 100,
                    ExpiresAt = DateTime.UtcNow.AddMonths(3),
                    MaxUsageCount = 1000,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Coupon
                {
                    Code = "SAVE50",
                    Title = "50 TL İndirim",
                    DiscountType = "Amount",
                    Amount = 50,
                    MinSpend = 200,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6),
                    MaxUsageCount = 500,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Coupon
                {
                    Code = "FIRST20",
                    Title = "İlk Randevu İndirimi",
                    DiscountType = "Percent",
                    Amount = 20,
                    MinSpend = 50,
                    ExpiresAt = DateTime.UtcNow.AddMonths(1),
                    MaxUsageCount = 200,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Coupon
                {
                    Code = "VIP15",
                    Title = "VIP Müşteri İndirimi",
                    DiscountType = "Percent",
                    Amount = 15,
                    MinSpend = 150,
                    ExpiresAt = DateTime.UtcNow.AddMonths(12),
                    MaxUsageCount = 100,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Coupon
                {
                    Code = "WEEKEND25",
                    Title = "Hafta Sonu Özel",
                    DiscountType = "Percent",
                    Amount = 25,
                    MinSpend = 100,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    MaxUsageCount = 300,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            await context.Coupons.AddRangeAsync(coupons);
            await context.SaveChangesAsync();
        }
    }
}
