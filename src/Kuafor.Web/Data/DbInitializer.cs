using Microsoft.AspNetCore.Identity;
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
        
        // Seed data oluştur
        await EnsureSeedDataAsync(context);
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

    private static async Task EnsureSeedDataAsync(ApplicationDbContext context)
    {
        // Eğer herhangi bir tablo doluysa seed işlemini atla
        if (context.Branches.Any() || context.Services.Any() || context.Stylists.Any() || 
            context.Coupons.Any() || context.Customers.Any() || context.Appointments.Any())
        {
            return;
        }

        // Şubeler
        var branches = new[]
        {
            new Branch { Name = "Merkez Şube", Address = "Kadıköy, İstanbul", Phone = "0216 123 45 67", IsActive = true },
            new Branch { Name = "Moda Şube", Address = "Moda, İstanbul", Phone = "0216 987 65 43", IsActive = true }
        };
        context.Branches.AddRange(branches);

        // Hizmetler
        var services = new[]
        {
            new Service { 
                Name = "Saç Kesimi", 
                DurationMin = 30, 
                Price = 150, 
                Category = "haircut",
                Description = "Profesyonel saç kesimi hizmeti",
                DetailedDescription = "Deneyimli kuaförlerimiz tarafından modern tekniklerle yapılan saç kesimi. Kişiye özel stil önerileri ile birlikte.",
                IconClass = "bi bi-scissors",
                IsActive = true,
                ShowOnHomePage = true,
                DisplayOrder = 1
            },
            new Service { 
                Name = "Bakım & Spa", 
                DurationMin = 60, 
                Price = 300, 
                Category = "styling",
                Description = "Saç bakımı ve spa tedavisi",
                DetailedDescription = "Saçınızın sağlığı için özel bakım ürünleri ile yapılan kapsamlı bakım ve spa tedavisi. Saç derisi masajı dahil.",
                IconClass = "bi bi-droplet",
                IsActive = true,
                ShowOnHomePage = true,
                DisplayOrder = 2
            },
            new Service { 
                Name = "Boya & Röfle", 
                DurationMin = 120, 
                Price = 500, 
                Category = "coloring",
                Description = "Profesyonel boya ve röfle",
                DetailedDescription = "Kaliteli boya ürünleri ile yapılan profesyonel boya ve röfle işlemleri. Renk danışmanlığı dahil.",
                IconClass = "bi bi-palette",
                IsActive = true,
                ShowOnHomePage = true,
                DisplayOrder = 3
            },
            new Service { 
                Name = "Manikür & Pedikür", 
                DurationMin = 45, 
                Price = 200, 
                Category = "beauty",
                Description = "El ve ayak bakımı",
                DetailedDescription = "Hijyenik ortamda yapılan manikür ve pedikür hizmeti. Tırnak bakımı ve oje uygulaması dahil.",
                IconClass = "bi bi-hand-index",
                IsActive = true,
                ShowOnHomePage = true,
                DisplayOrder = 4
            }
        };
        context.Services.AddRange(services);

        // İlk kaydetme - Branches ve Services için
        await context.SaveChangesAsync();

        // Kuaförler (BranchId'ye bağımlı)
        var stylists = new[]
        {
            new Stylist { FirstName = "Ahmet", LastName = "Özdoğan", BranchId = 1, IsActive = true, Rating = 4.5m, Bio = "Kesim ve stil uzmanı" },
            new Stylist { FirstName = "İbrahim", LastName = "Taşkın", BranchId = 1, IsActive = true, Rating = 4.2m, Bio = "Boya, röfle uzmanı" },
            new Stylist { FirstName = "Ahmet", LastName = "Ülker", BranchId = 2, IsActive = true, Rating = 4.8m, Bio = "Saç kesimi ve bakım" }
        };
        context.Stylists.AddRange(stylists);

        // Kuponlar
        var coupons = new[]
        {
            new Coupon { Code = "YENI10", Title = "Yeni Müşteri %10", DiscountType = "Percent", Amount = 10, IsActive = true, ExpiresAt = DateTime.Today.AddMonths(1) },
            new Coupon { Code = "SAK20", Title = "Saç Kesimi 20₺", DiscountType = "Amount", Amount = 20, IsActive = true, ExpiresAt = DateTime.Today.AddMonths(2) }
        };
        context.Coupons.AddRange(coupons);

        // Test müşterisi
        var customers = new[]
        {
            new Customer { FirstName = "Test", LastName = "Müşteri", Email = "test@example.com", Phone = "0555 123 45 67", IsActive = true }
        };
        context.Customers.AddRange(customers);

        // İkinci kaydetme - Stylists, Coupons ve Customers için
        await context.SaveChangesAsync();

        // Test randevuları (tüm foreign key'lere bağımlı)
        var appointments = new[]
        {
            new Appointment { ServiceId = 1, StylistId = 1, BranchId = 1, CustomerId = 1, StartAt = DateTime.Today.AddDays(1).AddHours(14), EndAt = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(30), Status = AppointmentStatus.Confirmed },
            new Appointment { ServiceId = 2, StylistId = 2, BranchId = 1, CustomerId = 1, StartAt = DateTime.Today.AddDays(2).AddHours(16), EndAt = DateTime.Today.AddDays(2).AddHours(17), Status = AppointmentStatus.Confirmed }
        };
        context.Appointments.AddRange(appointments);

        // Son kaydetme - Appointments için
        await context.SaveChangesAsync();

        // WorkingHours seed data
        if (!context.WorkingHours.Any())
        {
            var workingHours = new[]
            {
                // Pazartesi - Cuma: 09:00-18:00
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Tuesday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Wednesday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Thursday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Friday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                
                // Cumartesi: 10:00-16:00
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Saturday, OpenTime = new TimeSpan(10, 0, 0), CloseTime = new TimeSpan(16, 0, 0) },
                
                // Pazar: Kapalı
                new WorkingHours { BranchId = 1, DayOfWeek = DayOfWeek.Sunday, IsWorkingDay = false },
                
                // Moda Şube için benzer saatler
                new WorkingHours { BranchId = 2, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0) },
                // ... diğer günler
            };
            context.WorkingHours.AddRange(workingHours);
        }
        
        // Loyalty seed data
        if (!context.Loyalties.Any())
        {
            var loyalties = new[]
            {
                new Loyalty { CustomerId = 1, Points = 120, Tier = "Gümüş", TotalSpent = 450, AppointmentCount = 3 }
            };
            context.Loyalties.AddRange(loyalties);
        }
        
        await context.SaveChangesAsync();
    }
}
