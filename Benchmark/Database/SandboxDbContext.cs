using Microsoft.EntityFrameworkCore;

namespace Benchmark.Database;

public class SandboxDbContext : DbContext
{
    public SandboxDbContext() { }

    public SandboxDbContext(DbContextOptions<SandboxDbContext> options) : base(options) { }

    public DbSet<BulkImportBatch> BulkImportBatches => Set<BulkImportBatch>();
    public DbSet<BulkImportResult> BulkImportResults => Set<BulkImportResult>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeNameToken> EmployeeNameTokens => Set<EmployeeNameToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=RewardFlowDb_BenchmarkSandbox;User Id=sa;Password=sa12345678;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        // 1. BulkImportBatch
        modelBuilder.Entity<BulkImportBatch>(builder =>
        {
            builder.ToTable("BulkImportBatches");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedNever();
            builder.Property(b => b.Date).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(b => b.Status).IsRequired().HasMaxLength(50);
            builder.Property(b => b.TotalRecords).IsRequired().HasDefaultValue(0);
            builder.Property(b => b.RawPayloadJson).IsRequired().HasColumnType("nvarchar(max)");
            builder.HasIndex(b => b.Status);
        });

        // 2. BulkImportResult (BatchResults)
        modelBuilder.Entity<BulkImportResult>(builder =>
        {
            builder.ToTable("BulkImportResults");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();
            builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
            builder.Property(r => r.ErrorTypeCode).HasMaxLength(100);
            builder.Property(r => r.ErrorMessage).HasMaxLength(500);

            builder.HasOne(r => r.Batch)
                .WithMany(b => b.BulkImportResults)
                .HasForeignKey(r => r.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Employee)
                .WithMany(e => e.BulkImportResults)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 3. Employee
        modelBuilder.Entity<Employee>(builder =>
        {
            builder.ToTable("employees");
            builder.HasKey(e => e.EmployeeId);
            builder.Property(e => e.EmployeeId).HasColumnName("employee_id").ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            builder.Property(e => e.NationalNumber).HasColumnName("national_number").HasMaxLength(255);
            builder.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(255);
            builder.Property(e => e.Salary).HasColumnName("salary");
            builder.Property(e => e.FacultyId).HasColumnName("faculty_id");
            builder.Property(e => e.DepartmentId).HasColumnName("department_id");
            builder.Property(e => e.CreatedBy).HasColumnName("created_by");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.JobTitle).HasColumnName("job_title");
            builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            builder.Property(e => e.Status).HasColumnName("status");
            builder.Property(e => e.NationalNumberHash).HasColumnName("national_number_hash").HasMaxLength(255);
            builder.Property(e => e.AccountNumberHash).HasColumnName("account_number_hash").HasMaxLength(255);
        });

        // 4. EmployeeNameToken (NameToken)
        modelBuilder.Entity<EmployeeNameToken>(builder =>
        {
            builder.ToTable("employee_name_tokens");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(t => t.UserId).HasColumnName("user_id");
            builder.Property(t => t.TokenHashed).HasColumnName("token_hashed").HasMaxLength(64).IsRequired();
            builder.Property(t => t.N).HasColumnName("n");
            builder.Property(t => t.EmployeeId).HasColumnName("employee_id");

            builder.HasOne(t => t.Employee)
                .WithMany(e => e.NameTokens)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.UserId, t.TokenHashed });
        });
    }
}
