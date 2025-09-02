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
        
        // Seed data kaldırıldı - sadece rolleri ve admin kullanıcısı oluşturuluyor
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
}
