using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Employees.Data;

public class BulkImportBatchConfiguration : IEntityTypeConfiguration<BulkImportBatch>
{
    public void Configure(EntityTypeBuilder<BulkImportBatch> builder)
    {
        // 1. Table Name & Primary Key
        builder.ToTable("BulkImportBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        // 3. Date Configuration
        builder.Property(b => b.Date)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()"); // Uses SQL Server UTC default; use CURRENT_TIMESTAMP for PostgreSQL

        builder.Property<string>("Status")
            .IsRequired()
            .HasMaxLength(50); 

        // 5. Total Records Counter
        builder.Property(b => b.TotalRecords)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.RawPayloadJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)"); 
        
        builder.HasIndex("Status");
    }
}