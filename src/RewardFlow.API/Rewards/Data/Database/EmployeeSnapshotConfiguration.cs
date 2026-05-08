using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Common.Encryption;
using Reward_Flow_v2.Rewards.Data;


namespace RewardFlow.API.Employees.Data.Database;

public class EmployeeSnapshotEntityConfiguration : IEntityTypeConfiguration<EmployeeSnapshot>
{
   public void Configure(EntityTypeBuilder<EmployeeSnapshot> builder)
    {
        builder.ToTable("employee_snapshots");
        builder.HasKey(e => e.SnapshotId);
        builder.Property(e => e.SnapshotId).ValueGeneratedOnAdd();

        // Primary Key
        builder.Property(e => e.SnapshotId).HasColumnName("snapshot_id");

        // Snapshot Metadata
        builder.Property(e => e.SnapshotDate).HasColumnName("snapshot_date")
            .HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.EmployeeId).HasColumnName("employee_id").IsRequired();

        // Encrypted Fields with Hash Tracking
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        builder.Property(e => e.NationalNumber).HasColumnName("national_number")
            .HasMaxLength(255).IsUnicode(false).IsRequired(false)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        builder.Property(e => e.AccountNumber).HasColumnName("account_number")
            .HasMaxLength(255).IsUnicode(false).IsRequired(false)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        // Hash Fields (computed automatically via property setters)
        builder.Property(e => e.NationalNumberHash).HasColumnName("national_number_hash")
            .HasMaxLength(255).IsRequired(false);

        builder.Property(e => e.AccountNumberHash).HasColumnName("account_number_hash")
            .HasMaxLength(255).IsRequired(false);

        // Other Employee Data Fields - Flexible schema, no constraints
        builder.Property(e => e.Salary).HasColumnName("salary").IsRequired(false);
        builder.Property(e => e.JobTitle).HasColumnName("job_title").IsRequired(false);

        // Indexes for efficient querying by employee and date
        builder.HasIndex(e => e.EmployeeId)
            .HasDatabaseName("IX_EmployeeSnapshot_EmployeeId");

        builder.HasIndex(e => new { e.EmployeeId, e.SnapshotDate })
            .HasDatabaseName("IX_EmployeeSnapshot_EmployeeId_SnapshotDate");

    }
}