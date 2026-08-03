using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("rewards");
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");
        
        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        
        builder.Property(r => r.Total)
            .HasColumnName("total")
            .HasColumnType("decimal(9,2)");
        
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(r => r.LastUpdate)
            .HasColumnName("last_update")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by");
        
        builder.Property(r => r.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(r => r.RewardType)
            .HasColumnName("reward_type");
        
        builder.Property(r => r.NumberOfEmployees)
            .HasColumnName("number_of_employees");

        builder.HasMany(r => r.EmployeeRewards)
            .WithOne()
            .HasForeignKey(er => er.RewardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}