using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Common.Encryption;

namespace Reward_Flow_v2.Employees.Data.Database;

public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => new {e.TenantId, e.EmployeeId});
        
        builder.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
        
        builder.Property(e => e.EmployeeId).HasColumnName("employee_id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));
        
        builder.Property(e => e.NationalNumber).HasColumnName("national_number").HasMaxLength(255).IsUnicode(false).IsRequired(false)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));
        
        builder.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(255).IsUnicode(false)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));
        builder.Property(e => e.Salary).HasColumnName("salary").IsRequired(false);
        builder.Property(e => e.FacultyId).HasColumnName("faculty_id");
        builder.Property(e => e.DepartmentId).HasColumnName("department_id");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.JobTitle).HasColumnName("job_title");
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(e => e.Status).HasColumnName("status");
        builder.Property(e => e.NationalNumberHash).HasColumnName("national_number_hash").HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.AccountNumberHash).HasColumnName("account_number_hash").HasMaxLength(255);

        builder.HasIndex(e => new { e.CreatedBy, e.NationalNumberHash })
            .IsUnique()
            .HasDatabaseName("IX_Employee_CreatedBy_NationalNumberHash")
            .HasFilter("[national_number_hash] IS NOT NULL");
        
        builder.HasIndex(e => new { e.CreatedBy, e.AccountNumberHash })
            .IsUnique()
            .HasDatabaseName("IX_Employee_CreatedBy_AccountNumberHash")
            .HasFilter("[account_number_hash] IS NOT NULL");

        builder.ToTable("employees", t =>
            t.HasCheckConstraint("CHK_Salary_NonNegative", "[salary] >= 0 OR [salary] IS NULL"));
        
        // Shadow foreign key - no entity reference to avoid cross-namespace issues
        // Database-level FK constraint will be handled separately

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);

        builder.HasOne(e => e.Faculty)
            .WithMany(f => f.Employees)
            .HasForeignKey(e => e.FacultyId);

        builder.HasOne(e => e.JobTitleNavigation)
            .WithMany(j => j.Employees)
            .HasForeignKey(e => e.JobTitle);

        builder.HasOne(e => e.StatusNavigation)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.Status);
    }
}