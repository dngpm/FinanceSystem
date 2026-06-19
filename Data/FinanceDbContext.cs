using FinanceSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Data
{
    /// <summary>
    /// DbContext trung tâm  — quản lý toàn bộ bảng dữ liệu của hệ thống
    /// </summary>
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
            : base(options) { }

        // ── Các bảng (Table) trong database ──────────────────────────

        /// <summary>Bảng lưu lịch sử các khoản vay người dùng đã tính</summary>
        public DbSet<LoanInfo> LoanInfos { get; set; }

        public DbSet<OverdraftLoanInfo> OverdraftLoanInfos { get; set; }

        public DbSet<OverdraftUsageEntry> OverdraftUsageEntries { get; set; }

        /// <summary>Bảng lưu từng dòng lịch trả nợ (amortization)</summary>
        public DbSet<AmortizationRow> AmortizationRows { get; set; }

        /// <summary>Bảng lưu thông tin gửi tiết kiệm</summary>
        public DbSet<SavingsInfo> SavingsInfos { get; set; }

        /// <summary>Bảng lưu từng dòng lịch nhận lãi tiết kiệm</summary>
        public DbSet<SavingsRow> SavingsRows { get; set; }

        /// <summary>Bảng lưu các bút toán dòng tiền</summary>
        public DbSet<CashFlowEntry> CashFlowEntries { get; set; }

        /// <summary>Bảng lưu lịch sử tính toán dòng tiền của người dùng</summary>
        public DbSet<CashFlowHistory> CashFlowHistories { get; set; }

        /// <summary>Bảng lưu thông tin dự án đầu tư</summary>
        public DbSet<InvestmentProject> InvestmentProjects { get; set; }

        /// <summary>Bảng lưu tài khoản người dùng</summary>
        public DbSet<AppUser> AppUsers { get; set; }

        /// <summary>Bảng lưu vai trò</summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>Bảng trung gian phân quyền người dùng - vai trò</summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>Bảng lãi suất tiêu chuẩn (tiết kiệm & vay) — Admin quản lý</summary>
        public DbSet<InterestRateTable> InterestRateTables { get; set; }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        /// <summary>Bảng lưu lịch sử hoạt động người dùng</summary>
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        // ─────────────────────────────────────────────────────────────

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── LoanInfo ──────────────────────────────────────────────
            modelBuilder.Entity<LoanInfo>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CalculatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Principal)
                      .HasColumnType("decimal(18,0)")
                      .IsRequired();

                entity.Property(e => e.AnnualInterestRate)
                      .HasColumnType("decimal(8,6)")
                      .IsRequired();

                entity.Property(e => e.ServiceFee)
                      .HasColumnType("decimal(18,0)")
                      .HasDefaultValue(0);

                entity.Property(e => e.EarlyRepaymentPenaltyRate)
                      .HasColumnType("decimal(8,6)")
                      .HasDefaultValue(0.02m);
            });
            // ── OverdraftLoanInfo ─────────────────────────────────────
            modelBuilder.Entity<OverdraftLoanInfo>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.LoanName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.CreditLimit)
                      .HasColumnType("decimal(18,0)")
                      .IsRequired();

                entity.Property(e => e.AnnualInterestRate)
                      .HasColumnType("decimal(8,6)")
                      .IsRequired();
            });

            // ── OverdraftUsageEntry ───────────────────────────────────
            modelBuilder.Entity<OverdraftUsageEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UsedAmount)
                      .HasColumnType("decimal(18,0)")
                      .IsRequired();

                entity.Property(e => e.AnnualInterestRate)
                      .HasColumnType("decimal(8,6)")
                      .HasDefaultValue(0);

                entity.Property(e => e.Note)
                      .HasMaxLength(200);

                entity.HasOne(e => e.OverdraftLoan)
                      .WithMany(l => l.UsageEntries)
                      .HasForeignKey(e => e.OverdraftLoanInfoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── AmortizationRow ───────────────────────────────────────
            modelBuilder.Entity<AmortizationRow>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18,0)");
                entity.Property(e => e.PrincipalPayment).HasColumnType("decimal(18,0)");
                entity.Property(e => e.InterestPayment).HasColumnType("decimal(18,0)");
                entity.Property(e => e.ClosingBalance).HasColumnType("decimal(18,0)");
                entity.Property(e => e.EarlyRepaymentPenalty).HasColumnType("decimal(18,0)").HasDefaultValue(0);

                entity.HasOne(e => e.Loan)
                      .WithMany(l => l.Schedule)
                      .HasForeignKey(e => e.LoanInfoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SavingsInfo ───────────────────────────────────────────
            modelBuilder.Entity<SavingsInfo>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CalculatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Principal)
                      .HasColumnType("decimal(18,0)")
                      .IsRequired();

                entity.Property(e => e.AnnualInterestRate)
                      .HasColumnType("decimal(8,6)")
                      .IsRequired();

                entity.Property(e => e.DemandInterestRate)
                      .HasColumnType("decimal(8,6)")
                      .HasDefaultValue(0.001m);

                entity.Property(e => e.PaymentType)
                      .HasConversion<string>();

                entity.Property(e => e.DepositDate)
                      .IsRequired();
            });

            // ── SavingsRow ────────────────────────────────────────────
            modelBuilder.Entity<SavingsRow>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PeriodLabel)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18,0)");
                entity.Property(e => e.InterestEarned).HasColumnType("decimal(18,0)");
                entity.Property(e => e.InterestPaid).HasColumnType("decimal(18,0)");
                entity.Property(e => e.ClosingBalance).HasColumnType("decimal(18,0)");

                entity.HasOne<SavingsInfo>()
                      .WithMany()
                      .HasForeignKey(e => e.SavingsInfoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CashFlowEntry ─────────────────────────────────────────
            modelBuilder.Entity<CashFlowEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount).HasColumnType("decimal(18,0)").IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Type).HasConversion<string>();
            });

            // ── InvestmentProject ─────────────────────────────────────
            modelBuilder.Entity<InvestmentProject>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProjectName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.InitialInvestment).HasColumnType("decimal(18,0)");
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(8,6)");

                entity.Property(e => e.CashFlows)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<List<decimal>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<decimal>()
                      )
                      .HasColumnType("nvarchar(max)");
            });

            // ── CashFlowHistory ───────────────────────────────────────
            modelBuilder.Entity<CashFlowHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InitialInvestment).HasColumnType("decimal(18,0)");
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(8,6)");
                entity.Property(e => e.TotalInflow).HasColumnType("decimal(18,0)");
                entity.Property(e => e.TotalOutflow).HasColumnType("decimal(18,0)");
                entity.Property(e => e.NetCashFlow).HasColumnType("decimal(18,0)");
                entity.Property(e => e.NPV).HasColumnType("decimal(18,0)");
                entity.Property(e => e.IRR).HasColumnType("decimal(8,6)");
                entity.Property(e => e.Decision).HasMaxLength(500);
                entity.Property(e => e.CalculatedAt).HasDefaultValueSql("GETDATE()");
            });

            // ── AppUser ────────────────────────────────────────────────
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Email)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.PasswordHash)
                      .IsRequired();

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ── Role ───────────────────────────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RoleName)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(255);

                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // ── UserRole ───────────────────────────────────────────────
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                      .IsUnique();
            });

            // ── InterestRateTable ──────────────────────────────────────
            modelBuilder.Entity<InterestRateTable>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RateType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ProductType).HasMaxLength(40).IsRequired();
                entity.Property(e => e.TermLabel).HasMaxLength(50).IsRequired();
                entity.Property(e => e.AnnualRatePercent)
                      .HasColumnType("decimal(8,4)")
                      .IsRequired();
                entity.Property(e => e.Note).HasMaxLength(200);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            });

            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RequestType).HasMaxLength(30).IsRequired();
                entity.Property(e => e.ProductType).HasMaxLength(40).IsRequired();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,0)").IsRequired();
                entity.Property(e => e.AnnualRatePercent).HasColumnType("decimal(8,4)");
                entity.Property(e => e.CustomerNote).HasMaxLength(500);
                entity.Property(e => e.AdminNote).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ReviewedBy).HasMaxLength(50);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── UserActivityLog ────────────────────────────────────────
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Module).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Path).HasMaxLength(500);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });

            // ── Seed dữ liệu lãi suất mặc định ──────────────────────
            var now = new DateTime(2026, 1, 1);
            var seedRates = new List<InterestRateTable>
            {
                // --- Tiết kiệm ---
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "1 tháng",   TermMonths = 1,  AnnualRatePercent = 2.5m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "3 tháng",   TermMonths = 3,  AnnualRatePercent = 3.0m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "6 tháng",   TermMonths = 6,  AnnualRatePercent = 4.5m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "9 tháng",   TermMonths = 9,  AnnualRatePercent = 4.7m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "12 tháng",  TermMonths = 12, AnnualRatePercent = 5.2m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "18 tháng",  TermMonths = 18, AnnualRatePercent = 5.5m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "24 tháng",  TermMonths = 24, AnnualRatePercent = 5.7m,  Note = "",            UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"), RateType = "Savings", ProductType = FinancialProductType.SavingDeposit, TermLabel = "36 tháng",  TermMonths = 36, AnnualRatePercent = 6.0m,  Note = "Khóa hạn",   UpdatedAt = now, UpdatedBy = "system" },
                // --- Vay ---
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000001"), RateType = "Loan",    ProductType = FinancialProductType.UnsecuredLoan, TermLabel = "Vay tín chấp",  TermMonths = 36,  AnnualRatePercent = 14.0m,  Note = "Không cần tài sản bảo đảm",  UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000002"), RateType = "Loan",    ProductType = FinancialProductType.OverdraftLoan, TermLabel = "Vay thấu chi", TermMonths = 12,  AnnualRatePercent = 16.0m,  Note = "Tính lãi trên số tiền đã sử dụng",  UpdatedAt = now, UpdatedBy = "system" },
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000003"), RateType = "Loan",    ProductType = FinancialProductType.InstallmentLoan, TermLabel = "Vay trả góp", TermMonths = 60,  AnnualRatePercent = 10.5m, Note = "Trả góp định kỳ", UpdatedAt = now, UpdatedBy = "system" },
            };
            modelBuilder.Entity<InterestRateTable>().HasData(seedRates);
        }
    }
}
