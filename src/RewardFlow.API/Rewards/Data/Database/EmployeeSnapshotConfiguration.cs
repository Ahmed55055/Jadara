using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Common.Encryption;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class EmployeeSnapshotConfiguration : IEntityTypeConfiguration<EmployeeSnapshot>
{
    public void Configure(EntityTypeBuilder<EmployeeSnapshot> builder)
    {
        const bool isNationalNumRequired = false;
        const bool isAccountNumRequired = false;
        
        builder.ToTable("employee_snapshots");
        builder.HasKey(e => e.SnapshotId);
        
        // Primary Key
        builder.Property(e => e.SnapshotId)
            .HasColumnName("snapshot_id")
            .ValueGeneratedOnAdd();

        // Snapshot Metadata
        builder.Property(e => e.SnapshotDate)
            .HasColumnName("snapshot_date")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        // Encrypted Fields with Hash Tracking
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        builder.Property(e => e.NationalNumber)
            .HasColumnName("national_number")
            .HasField("_nationalNumber")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired(isNationalNumRequired)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        builder.Property(e => e.AccountNumber)
            .HasField("_accountNumber")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("account_number")
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired(isAccountNumRequired)
            .HasConversion(
                v => AesEncryptionService.EncryptString(v),
                v => AesEncryptionService.DecryptString(v));

        builder.Property(e => e.NationalNumberHash)
            .HasColumnName("national_number_hash")
            .HasMaxLength(255)
            .IsRequired(isNationalNumRequired);

        builder.Property(e => e.AccountNumberHash
            ).HasColumnName("account_number_hash")
            .HasMaxLength(255)
            .IsRequired(isAccountNumRequired);

        builder.Property(e => e.Salary)
            .HasColumnName("salary")
            .HasColumnType("decimal(9,2)")
            .IsRequired(false);
        
        builder.Property(e => e.JobTitle)
            .HasColumnName("job_title")
            .IsRequired(false);

        builder.HasIndex(e => new { e.EmployeeId, e.SnapshotDate })
            .HasDatabaseName("IX_EmployeeSnapshot_EmployeeId_SnapshotDate");
    }
}