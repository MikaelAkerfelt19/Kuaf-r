using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    
    // Entity DbSets
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Stylist> Stylists { get; set; } = null!;
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Testimonial> Testimonials { get; set; } = null!;
    
 
    // WorkingHours tablosu zaten veritabanında mevcut
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Coupon Code unique constraint
        builder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();
            
        // Appointment time conflict prevention
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.StylistId, a.StartAt, a.EndAt });
            
        // Customer UserId unique constraint
        builder.Entity<Customer>()
            .HasIndex(c => c.UserId)
            .IsUnique();
            
        // Stylist BranchId foreign key
        builder.Entity<Stylist>()
            .HasOne(s => s.Branch)
            .WithMany(b => b.Stylists)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Appointment foreign keys
        builder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<Appointment>()
            .HasOne(a => a.Stylist)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.StylistId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<Appointment>()
            .HasOne(a => a.Branch)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // WorkingHours tablosu zaten veritabanında mevcut
            
        // : Payment foreign key
        builder.Entity<Payment>()
            .HasOne(p => p.Appointment)
            .WithMany(a => a.Payments)
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // WorkingHours tablosu zaten veritabanında mevcut
            
        //  Notification index (performans için)
        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            
        //  Report index (performans için)
        builder.Entity<Report>()
            .HasIndex(r => new { r.Type, r.CreatedAt });
    }
}
