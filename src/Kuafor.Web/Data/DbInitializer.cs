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
        
        // Mesaj şablonlarını oluştur
        await EnsureMessageTemplatesAsync(context);
        
        // WhatsApp şablonlarını oluştur
        await EnsureWhatsAppTemplatesAsync(context);
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

    private static Task EnsureBasicDataAsync(ApplicationDbContext context)
    {
        // Sadece admin kullanıcı ve roller oluşturulacak
        // Diğer veriler admin panelinden eklenecek
        // Mock veriler kaldırıldı - profesyonel kullanım için
        return Task.CompletedTask;
    }

    private static async Task EnsureMessageTemplatesAsync(ApplicationDbContext context)
    {
        // Mesaj şablonları zaten var mı kontrol et
        if (await context.MessageTemplates.AnyAsync())
            return;

        var templates = new[]
        {
            new MessageTemplate
            {
                Name = "Hoş Geldin Mesajı",
                Type = "WhatsApp",
                Content = "Merhaba {{FirstName}}! 🎉 Kuafor salonumuza hoş geldiniz. Size en iyi hizmeti sunmak için buradayız. Randevu almak için bizi arayabilirsiniz. 💇‍♀️✨",
                Description = "Yeni müşteriler için hoş geldin mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new MessageTemplate
            {
                Name = "Randevu Hatırlatması",
                Type = "WhatsApp",
                Content = "Merhaba {{FirstName}}! 📅 Yarın saat {{Time}}'da {{Service}} randevunuz bulunmaktadır. Lütfen 15 dakika önce salonda olunuz. 🕐",
                Description = "Randevu hatırlatma mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
            },
            new MessageTemplate
            {
                Name = "Randevu İptal Bildirimi",
                Type = "WhatsApp",
                Content = "Merhaba {{FirstName}}! 😔 Randevunuz iptal edilmiştir. Yeni bir randevu için bizi arayabilirsiniz. 📞",
                Description = "Randevu iptal bildirimi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new MessageTemplate
            {
                Name = "Doğum Günü Mesajı",
                Type = "WhatsApp",
                Content = "🎂🎉 Doğum gününüz kutlu olsun {{FirstName}}! Bugün özel gününüzde size %20 indirimli hizmet sunuyoruz. Hemen randevu alın! 🎁",
                Description = "Doğum günü tebrik mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new MessageTemplate
            {
                Name = "Kampanya Duyurusu",
                Type = "WhatsApp",
                Content = "🎯 {{FirstName}}, özel kampanyamızı kaçırma! Bu hafta sonu tüm hizmetlerde %25 indirim. Hemen randevu al! ⚡",
                Description = "Kampanya duyuru mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new MessageTemplate
            {
                Name = "Kupon Bildirimi",
                Type = "WhatsApp",
                Content = "🎁 {{FirstName}}! Size özel bir kupon hazırladık. Kod: {{CouponCode}} - {{Discount}}% indirim. Son kullanma: {{ExpiryDate}} 📱",
                Description = "Kupon bildirim mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
            },
            new MessageTemplate
            {
                Name = "SMS Hoş Geldin",
                Type = "SMS",
                Content = "Merhaba {{FirstName}}! Kuafor salonumuza hoş geldiniz. Randevu için: 0212 XXX XX XX",
                Description = "SMS hoş geldin mesajı",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new MessageTemplate
            {
                Name = "SMS Randevu Hatırlatma",
                Type = "SMS",
                Content = "{{FirstName}}, yarın {{Time}} randevunuz var. 15 dk önce geliniz.",
                Description = "SMS randevu hatırlatma",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
        await context.MessageTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
        }
        
    private static async Task EnsureWhatsAppTemplatesAsync(ApplicationDbContext context)
    {
        // WhatsApp şablonları zaten var mı kontrol et
        if (await context.WhatsAppTemplates.AnyAsync())
            return;

        var whatsappTemplates = new[]
        {
            new WhatsAppTemplate
            {
                Name = "hoşgeldin_mesajı",
                Category = "UTILITY",
                Content = "Merhaba {{FirstName}}! 🎉 Kuafor salonumuza hoş geldiniz. Size en iyi hizmeti sunmak için buradayız. Randevu almak için bizi arayabilirsiniz. 💇‍♀️✨",
                Description = "Yeni müşteriler için hoş geldin mesajı",
                Language = "tr",
                Status = "APPROVED",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new WhatsAppTemplate
            {
                Name = "randevu_hatirlatma",
                Category = "UTILITY",
                Content = "Merhaba {{FirstName}}! 📅 Yarın saat {{Time}}'da {{Service}} randevunuz bulunmaktadır. Lütfen 15 dakika önce salonda olunuz. 🕐",
                Description = "Randevu hatırlatma mesajı",
                Language = "tr",
                Status = "APPROVED",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new WhatsAppTemplate
            {
                Name = "randevu_iptal_bildirimi",
                Category = "UTILITY",
                Content = "Merhaba {{FirstName}}! 😔 Randevunuz iptal edilmiştir. Yeni bir randevu için bizi arayabilirsiniz. 📞",
                Description = "Randevu iptal bildirimi",
                Language = "tr",
                Status = "APPROVED",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new WhatsAppTemplate
            {
                Name = "dogum_gunu_mesaji",
                Category = "MARKETING",
                Content = "🎂🎉 Doğum gününüz kutlu olsun {{FirstName}}! Bugün özel gününüzde size %20 indirimli hizmet sunuyoruz. Hemen randevu alın! 🎁",
                Description = "Doğum günü tebrik mesajı",
                Language = "tr",
                Status = "APPROVED",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new WhatsAppTemplate
            {
                Name = "kampanya_duyurusu",
                Category = "MARKETING",
                Content = "🎯 {{FirstName}}, özel kampanyamızı kaçırma! Bu hafta sonu tüm hizmetlerde %25 indirim. Hemen randevu al! ⚡",
                Description = "Kampanya duyuru mesajı",
                Language = "tr",
                Status = "APPROVED",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
            new WhatsAppTemplate
            {
                Name = "kupon_bildirimi",
                Category = "MARKETING",
                Content = "🎁 {{FirstName}}! Size özel bir kupon hazırladık. Kod: {{CouponCode}} - {{Discount}}% indirim. Son kullanma: {{ExpiryDate}} 📱",
                Description = "Kupon bildirim mesajı",
                Language = "tr",
                Status = "APPROVED",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
        await context.WhatsAppTemplates.AddRangeAsync(whatsappTemplates);
            await context.SaveChangesAsync();
    }
}
