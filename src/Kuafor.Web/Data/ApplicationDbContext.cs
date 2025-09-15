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
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Testimonial> Testimonials { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Product> Products { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageService> PackageServices { get; set; }
    public DbSet<PackageSale> PackageSales { get; set; }
    public DbSet<MessageGroup> MessageGroups { get; set; }
    public DbSet<MessageGroupMember> MessageGroupMembers { get; set; }
    public DbSet<MessageCampaign> MessageCampaigns { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    
    // Yeni eklenen DbSets
    public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
    public DbSet<Loyalty> Loyalties { get; set; } = null!;
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; } = null!;
    public DbSet<StylistWorkingHours> StylistWorkingHours { get; set; } = null!;
    
    // Stok yönetimi DbSets
    public DbSet<StockTransaction> StockTransactions { get; set; } = null!;
    
    // Müşteri analitikleri DbSets
    public DbSet<CustomerAnalytics> CustomerAnalytics { get; set; } = null!;
    public DbSet<CustomerSegment> CustomerSegments { get; set; } = null!;
    public DbSet<CustomerBehavior> CustomerBehaviors { get; set; } = null!;
    public DbSet<CustomerPreference> CustomerPreferences { get; set; } = null!;
    
    // Personel yönetimi DbSets
    public DbSet<StaffPerformance> StaffPerformances { get; set; } = null!;
    public DbSet<StaffSalary> StaffSalaries { get; set; } = null!;
    public DbSet<StaffTraining> StaffTrainings { get; set; } = null!;
    public DbSet<StaffAttendance> StaffAttendances { get; set; } = null!;
     public DbSet<StaffEvaluation> StaffEvaluations { get; set; } = null!;
    public DbSet<StaffGoal> StaffGoals { get; set; } = null!;
    
    // Finansal analitikler DbSets
    public DbSet<FinancialCategory> FinancialCategories { get; set; } = null!;
    public DbSet<CostAnalysis> CostAnalyses { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<BudgetItem> BudgetItems { get; set; } = null!;
    public DbSet<CashFlow> CashFlows { get; set; } = null!;
    public DbSet<FinancialReport> FinancialReports { get; set; } = null!;
    
    // Pazarlama DbSets
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<CampaignTarget> CampaignTargets { get; set; } = null!;
    public DbSet<CampaignMessage> CampaignMessages { get; set; } = null!;
    public DbSet<CampaignPerformance> CampaignPerformances { get; set; } = null!;
    
    // Marketing entities from Marketing.cs
    public DbSet<MarketingCampaign> MarketingCampaigns { get; set; } = null!;
    public DbSet<CampaignRecipient> CampaignRecipients { get; set; } = null!;
    public DbSet<CampaignTemplate> CampaignTemplates { get; set; } = null!;
    public DbSet<ReferralProgram> ReferralPrograms { get; set; } = null!;
    public DbSet<Referral> Referrals { get; set; } = null!;
    public DbSet<BirthdayCampaign> BirthdayCampaigns { get; set; } = null!;
    public DbSet<BirthdayMessage> BirthdayMessages { get; set; } = null!;
    
    // JWT Refresh Tokens
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure decimal precision for all decimal properties
        builder.Entity<Appointment>()
            .Property(a => a.TotalPrice)
            .HasPrecision(18, 2);
            
        builder.Entity<Appointment>()
            .Property(a => a.DiscountAmount)
            .HasPrecision(18, 2);
            
        builder.Entity<Appointment>()
            .Property(a => a.FinalPrice)
            .HasPrecision(18, 2);
            
        builder.Entity<Coupon>()
            .Property(c => c.Amount)
            .HasPrecision(18, 2);
            
        builder.Entity<Coupon>()
            .Property(c => c.MinSpend)
            .HasPrecision(18, 2);
            
        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);
            
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);
            
        builder.Entity<Service>()
            .Property(s => s.PriceFrom)
            .HasPrecision(18, 2);
            
        builder.Entity<Stylist>()
            .Property(s => s.Rating)
            .HasPrecision(3, 1);
        
        // Coupon Code unique constraint
        builder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();
            
        // Appointment time conflict prevention
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.StylistId, a.StartAt, a.EndAt })
            .HasDatabaseName("IX_Appointments_StylistId_StartAt_EndAt");
            
        // Performance indexes
        builder.Entity<Appointment>()
            .HasIndex(a => a.StartAt)
            .HasDatabaseName("IX_Appointments_StartAt");
            
        builder.Entity<Appointment>()
            .HasIndex(a => a.CustomerId)
            .HasDatabaseName("IX_Appointments_CustomerId");
            
        builder.Entity<Appointment>()
            .HasIndex(a => a.StylistId)
            .HasDatabaseName("IX_Appointments_StylistId");
            
        builder.Entity<Appointment>()
            .HasIndex(a => a.BranchId)
            .HasDatabaseName("IX_Appointments_BranchId");
            
        builder.Entity<Appointment>()
            .HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");
            
        // Customer indexes
        builder.Entity<Customer>()
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Customers_UserId");
            
        builder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .HasDatabaseName("IX_Customers_Email");
            
        builder.Entity<Customer>()
            .HasIndex(c => c.Phone)
            .HasDatabaseName("IX_Customers_Phone");
            
        // Stylist indexes
        builder.Entity<Stylist>()
            .HasIndex(s => s.BranchId)
            .HasDatabaseName("IX_Stylists_BranchId");
            
        builder.Entity<Stylist>()
            .HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Stylists_IsActive");
            
        // Service indexes
        builder.Entity<Service>()
            .HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Services_IsActive");
            
        // Service configuration - Eksik kolonları ekle
        builder.Entity<Service>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ShowOnHomePage).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.DetailedDescription).HasMaxLength(1000);
            entity.Property(e => e.IconClass).HasMaxLength(50);
        });
            
        // Branch indexes
        builder.Entity<Branch>()
            .HasIndex(b => b.IsActive)
            .HasDatabaseName("IX_Branches_IsActive");
            
        // Testimonial configuration - Eksik kolonları ekle
        builder.Entity<Testimonial>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.ShowOnHomePage).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AdminNotes).HasMaxLength(1000);
        });
            
        // Payment indexes
        builder.Entity<Payment>()
            .HasIndex(p => p.AppointmentId)
            .HasDatabaseName("IX_Payments_AppointmentId");
            
        // Notification indexes
        builder.Entity<Notification>()
            .HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId");
            
        builder.Entity<Notification>()
            .HasIndex(n => n.CreatedAt)
            .HasDatabaseName("IX_Notifications_CreatedAt");
            
        // RefreshToken indexes
        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");
            
        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");
            
        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
            
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

        // WorkingHours configuration
        builder.Entity<WorkingHours>()
            .HasOne(w => w.Branch)
            .WithMany()
            .HasForeignKey(w => w.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Loyalty configuration
        builder.Entity<Loyalty>()
            .HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // LoyaltyTransaction configuration
        builder.Entity<LoyaltyTransaction>()
            .HasOne(lt => lt.Customer)
            .WithMany()
            .HasForeignKey(lt => lt.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product decimal precision
        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
            
        builder.Entity<Product>()
            .Property(p => p.CommissionPercentage)
            .HasPrecision(5, 2);

        // Receipt decimal precision
        builder.Entity<Receipt>()
            .Property(r => r.TotalAmount)
            .HasPrecision(18, 2);
            
        builder.Entity<Receipt>()
            .Property(r => r.PaidAmount)
            .HasPrecision(18, 2);
            
        builder.Entity<Receipt>()
            .Property(r => r.RemainingAmount)
            .HasPrecision(18, 2);

        // ReceiptItem decimal precision
        builder.Entity<ReceiptItem>()
            .Property(ri => ri.UnitPrice)
            .HasPrecision(18, 2);
            
        builder.Entity<ReceiptItem>()
            .Property(ri => ri.TotalPrice)
            .HasPrecision(18, 2);

        // Package decimal precision
        builder.Entity<Package>()
            .Property(p => p.TotalPrice)
            .HasPrecision(18, 2);
            
        builder.Entity<Package>()
            .Property(p => p.SessionUnitPrice)
            .HasPrecision(18, 2);

        // FinancialTransaction decimal precision
        builder.Entity<FinancialTransaction>()
            .Property(ft => ft.Amount)
            .HasPrecision(18, 2);

        // Foreign key relationships
        builder.Entity<Receipt>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Receipt)
            .WithMany(r => r.ReceiptItems)
            .HasForeignKey(ri => ri.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Service)
            .WithMany()
            .HasForeignKey(ri => ri.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Product)
            .WithMany(p => p.ReceiptItems)
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PackageService>()
            .HasOne(ps => ps.Package)
            .WithMany(p => p.PackageServices)
            .HasForeignKey(ps => ps.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PackageService>()
            .HasOne(ps => ps.Service)
            .WithMany()
            .HasForeignKey(ps => ps.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PackageSale>()
            .HasOne(ps => ps.Package)
            .WithMany(p => p.PackageSales)
            .HasForeignKey(ps => ps.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PackageSale>()
            .HasOne(ps => ps.Customer)
            .WithMany()
            .HasForeignKey(ps => ps.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MessageGroupMember>()
            .HasOne(mgm => mgm.MessageGroup)
            .WithMany(mg => mg.Members)
            .HasForeignKey(mgm => mgm.MessageGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MessageGroupMember>()
            .HasOne(mgm => mgm.Customer)
            .WithMany()
            .HasForeignKey(mgm => mgm.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MessageCampaign>()
            .HasOne(mc => mc.MessageGroup)
            .WithMany()
            .HasForeignKey(mc => mc.MessageGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MessageRecipient>()
            .HasOne(mr => mr.MessageCampaign)
            .WithMany(mc => mc.Recipients)
            .HasForeignKey(mr => mr.MessageCampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MessageRecipient>()
            .HasOne(mr => mr.Customer)
            .WithMany()
            .HasForeignKey(mr => mr.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Customer)
            .WithMany()
            .HasForeignKey(ft => ft.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Stylist)
            .WithMany()
            .HasForeignKey(ft => ft.StylistId)
            .OnDelete(DeleteBehavior.Restrict);

        // StylistWorkingHours decimal precision
        builder.Entity<StylistWorkingHours>()
            .Property(swh => swh.OpenTime)
            .HasConversion(
                v => v.Ticks,
                v => new TimeSpan(v));
                
        builder.Entity<StylistWorkingHours>()
            .Property(swh => swh.CloseTime)
            .HasConversion(
                v => v.Ticks,
                v => new TimeSpan(v));
                
        builder.Entity<StylistWorkingHours>()
            .Property(swh => swh.BreakStart)
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? new TimeSpan(v.Value) : null);
                
        builder.Entity<StylistWorkingHours>()
            .Property(swh => swh.BreakEnd)
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? new TimeSpan(v.Value) : null);

        // StylistWorkingHours foreign key
        builder.Entity<StylistWorkingHours>()
            .HasOne(swh => swh.Stylist)
            .WithMany(s => s.WorkingHours)
            .HasForeignKey(swh => swh.StylistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stok yönetimi konfigürasyonları
        builder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Product entity güncellemeleri
        builder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Stock).HasDefaultValue(0);
            entity.Property(e => e.MinimumStock).HasDefaultValue(5);
            entity.Property(e => e.MaximumStock).HasDefaultValue(100);
            entity.Property(e => e.Supplier).HasMaxLength(100);
            entity.Property(e => e.SupplierCode).HasMaxLength(50);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
        });
        
        // Müşteri analitikleri konfigürasyonları
        builder.Entity<CustomerAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Segment).HasMaxLength(50);
            entity.Property(e => e.LifecycleStage).HasMaxLength(50);
            entity.Property(e => e.PreferredDayOfWeek).HasMaxLength(20);
            entity.Property(e => e.PreferredTimeSlot).HasMaxLength(20);
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.PreferredService)
                .WithMany()
                .HasForeignKey(e => e.PreferredServiceId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.PreferredStylist)
                .WithMany()
                .HasForeignKey(e => e.PreferredStylistId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.PreferredBranch)
                .WithMany()
                .HasForeignKey(e => e.PreferredBranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<CustomerSegment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(20);
        });
        
        builder.Entity<CustomerBehavior>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(200);
            entity.Property(e => e.PageUrl).HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(100);
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<CustomerPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferenceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PreferenceValue).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Personel yönetimi konfigürasyonları
        builder.Entity<StaffPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);
            entity.Property(e => e.AverageTicketValue).HasPrecision(18, 2);
            entity.Property(e => e.CommissionEarned).HasPrecision(18, 2);
            entity.Property(e => e.AverageRating).HasPrecision(3, 1);
            entity.Property(e => e.AverageServiceTime).HasPrecision(8, 2);
            entity.Property(e => e.UtilizationRate).HasPrecision(5, 2);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<StaffSalary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.PerformanceBonus).HasPrecision(18, 2);
            entity.Property(e => e.SalesBonus).HasPrecision(18, 2);
            entity.Property(e => e.AttendanceBonus).HasPrecision(18, 2);
            entity.Property(e => e.TaxDeduction).HasPrecision(18, 2);
            entity.Property(e => e.InsuranceDeduction).HasPrecision(18, 2);
            entity.Property(e => e.OtherDeductions).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<StaffTraining>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TrainingDate).IsRequired();
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<StaffAttendance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.CheckInTime).IsRequired();
            entity.Property(e => e.CheckOutTime).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        
        builder.Entity<StaffEvaluation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EvaluationDate).IsRequired();
            entity.Property(e => e.TechnicalSkills).HasPrecision(3, 1);
            entity.Property(e => e.CustomerService).HasPrecision(3, 1);
            entity.Property(e => e.Teamwork).HasPrecision(3, 1);
            entity.Property(e => e.Punctuality).HasPrecision(3, 1);
            entity.Property(e => e.Professionalism).HasPrecision(3, 1);
            entity.Property(e => e.Communication).HasPrecision(3, 1);
            entity.Property(e => e.ProblemSolving).HasPrecision(3, 1);
            entity.Property(e => e.Adaptability).HasPrecision(3, 1);
            entity.Property(e => e.OverallScore).HasPrecision(3, 1);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Evaluator)
                .WithMany()
                .HasForeignKey(e => e.EvaluatorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
       
        
        builder.Entity<StaffGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GoalType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TargetValue).HasPrecision(18, 2);
            entity.Property(e => e.CurrentValue).HasPrecision(18, 2);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Stylist)
                .WithMany()
                .HasForeignKey(e => e.StylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Finansal analitikler konfigürasyonları
        builder.Entity<FinancialCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.DisplayOrder).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.ParentCategory)
                .WithMany()
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<CostAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PeriodStart).IsRequired();
            entity.Property(e => e.PeriodEnd).IsRequired();
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.ServiceCost).HasPrecision(18, 2);
            entity.Property(e => e.ProductCost).HasPrecision(18, 2);
            entity.Property(e => e.OtherCost).HasPrecision(18, 2);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.Quarter).IsRequired();
            entity.Property(e => e.RevenueBudget).HasPrecision(18, 2);
            entity.Property(e => e.ExpenseBudget).HasPrecision(18, 2);
            entity.Property(e => e.ProfitBudget).HasPrecision(18, 2);
            entity.Property(e => e.ActualRevenue).HasPrecision(18, 2);
            entity.Property(e => e.ActualExpense).HasPrecision(18, 2);
            entity.Property(e => e.ActualProfit).HasPrecision(18, 2);
            entity.Property(e => e.RevenueVariance).HasPrecision(18, 2);
            entity.Property(e => e.ExpenseVariance).HasPrecision(18, 2);
            entity.Property(e => e.ProfitVariance).HasPrecision(18, 2);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<BudgetItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BudgetedAmount).HasPrecision(18, 2);
            entity.Property(e => e.ActualAmount).HasPrecision(18, 2);
            entity.Property(e => e.Variance).HasPrecision(18, 2);
            entity.Property(e => e.VariancePercentage).HasPrecision(5, 2);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.BudgetItems)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<CashFlow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.CashIn).HasPrecision(18, 2);
            entity.Property(e => e.CardIn).HasPrecision(18, 2);
            entity.Property(e => e.TransferIn).HasPrecision(18, 2);
            entity.Property(e => e.OtherIn).HasPrecision(18, 2);
            entity.Property(e => e.CashOut).HasPrecision(18, 2);
            entity.Property(e => e.CardOut).HasPrecision(18, 2);
            entity.Property(e => e.TransferOut).HasPrecision(18, 2);
            entity.Property(e => e.OtherOut).HasPrecision(18, 2);
            entity.Property(e => e.NetCashFlow).HasPrecision(18, 2);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<FinancialReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ReportType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReportDate).IsRequired();
            entity.Property(e => e.PeriodStart).IsRequired();
            entity.Property(e => e.PeriodEnd).IsRequired();
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.Format).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Pazarlama konfigürasyonları
        builder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CampaignType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Marketing entities decimal precision
        builder.Entity<MarketingCampaign>(entity =>
        {
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            entity.Property(e => e.CostPerMessage).HasPrecision(18, 2);
        });
        
        builder.Entity<BirthdayCampaign>(entity =>
        {
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        });
        
        builder.Entity<ReferralProgram>(entity =>
        {
            entity.Property(e => e.ReferrerReward).HasPrecision(18, 2);
            entity.Property(e => e.RefereeReward).HasPrecision(18, 2);
            entity.Property(e => e.MinSpentAmount).HasPrecision(18, 2);
        });
        
        builder.Entity<Referral>(entity =>
        {
            entity.Property(e => e.ReferrerRewardAmount).HasPrecision(18, 2);
            entity.Property(e => e.RefereeRewardAmount).HasPrecision(18, 2);
        });
        
        builder.Entity<CustomerSegment>(entity =>
        {
            entity.Property(e => e.MinMonetaryScore).HasPrecision(18, 2);
            entity.Property(e => e.MaxMonetaryScore).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
        });
        
        builder.Entity<CustomerAnalytics>(entity =>
        {
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
            entity.Property(e => e.AverageTicketValue).HasPrecision(18, 2);
            entity.Property(e => e.LifetimeValue).HasPrecision(18, 2);
            entity.Property(e => e.MonetaryScore).HasPrecision(18, 2);
        });
        
        builder.Entity<CostAnalysis>(entity =>
        {
            entity.Property(e => e.LaborCost).HasPrecision(18, 2);
            entity.Property(e => e.MaterialCost).HasPrecision(18, 2);
            entity.Property(e => e.OverheadCost).HasPrecision(18, 2);
            entity.Property(e => e.OtherCosts).HasPrecision(18, 2);
            entity.Property(e => e.ServiceCost).HasPrecision(18, 2);
            entity.Property(e => e.ProductCost).HasPrecision(18, 2);
            entity.Property(e => e.OtherCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            entity.Property(e => e.ProfitMargin).HasPrecision(18, 2);
        });
        
        builder.Entity<CashFlow>(entity =>
        {
            entity.Property(e => e.CashBalance).HasPrecision(18, 2);
        });
        
        builder.Entity<StaffSalary>(entity =>
        {
            entity.Property(e => e.QualityBonus).HasPrecision(18, 2);
        });
        
        builder.Entity<StockTransaction>(entity =>
        {
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
        });
        
        builder.Entity<CampaignTarget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TargetValue).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Targets)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<CampaignMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sequence).IsRequired();
            entity.Property(e => e.MessageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<CampaignPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SentCount).IsRequired();
            entity.Property(e => e.DeliveredCount).IsRequired();
            entity.Property(e => e.OpenedCount).IsRequired();
            entity.Property(e => e.ClickedCount).IsRequired();
            entity.Property(e => e.ConvertedCount).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Performance)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // CouponUsage configuration
        builder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.UsedAt).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Coupon)
                .WithMany()
                .HasForeignKey(e => e.CouponId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Referrals configuration - Foreign key constraint hatasını önlemek için
        builder.Entity<Referral>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferrerRewardAmount).HasPrecision(18, 2);
            entity.Property(e => e.RefereeRewardAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ReferralCode).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.ReferralProgram)
                .WithMany(rp => rp.Referrals)
                .HasForeignKey(e => e.ReferralProgramId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ReferrerCustomer)
                .WithMany()
                .HasForeignKey(e => e.ReferrerCustomerId)
                .OnDelete(DeleteBehavior.NoAction); // CASCADE yerine NO ACTION
                
            entity.HasOne(e => e.RefereeCustomer)
                .WithMany()
                .HasForeignKey(e => e.RefereeCustomerId)
                .OnDelete(DeleteBehavior.NoAction); // CASCADE yerine NO ACTION
        });
    }
}
