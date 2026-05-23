using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class EmployeeSessionRewardConfiguration : IEntityTypeConfiguration<EmployeeSessionReward>
{
    public void Configure(EntityTypeBuilder<EmployeeSessionReward> builder)
    {
        builder.ToTable("employee_session_rewards");
        builder.HasKey(esr => new { esr.SessionRewardId, esr.EmployeeId });

        builder.Property(esr => esr.SessionRewardId)
            .HasColumnName("session_reward_id")
            .IsRequired();

        builder.Property(esr => esr.EmployeeSnapshotId)
            .HasColumnName("employee_snapshot_id")
            .IsRequired();

        builder.Property(esr => esr.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(esr => esr.SessionsCount)
            .HasColumnName("sessions_count");
        
        builder.Property(esr => esr.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(esr => esr.EmployeeSnapshot)
            .WithMany()
            .HasForeignKey(esr => esr.EmployeeSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}