using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class EmployeeRewardConfiguration : IEntityTypeConfiguration<EmployeeReward>
{
    public void Configure(EntityTypeBuilder<EmployeeReward> builder)
    {
        builder.ToTable("employee_rewards");
        builder.HasKey(er => new {er.RewardId,er.EmployeeId});
       
        builder.Property(er=> er.EmployeeId).HasColumnName("employee_id");
        builder.Property(er => er.RewardId).HasColumnName("reward_id");
        builder.Property(er => er.EmployeeSnapshotId).HasColumnName("snapshot_id");
        builder.Property(er => er.Amount).HasColumnName("amount").HasColumnType("decimal(9,2)");
        builder.Property(er => er.IsUpdated).HasColumnName("is_updated");
        
        builder.HasOne<Reward>()
            .WithMany()
            .HasForeignKey(er => er.RewardId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(er => er.EmployeeSnapshot)
            .WithMany() 
            .HasForeignKey(er => er.EmployeeSnapshotId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}