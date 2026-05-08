using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class EmployeeSessionRewardEntityConfiguration : IEntityTypeConfiguration<EmployeeSessionRewardEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeSessionRewardEntity> builder)
    {
        builder.ToTable("employee_session_rewards");
        builder.HasKey(esr => esr.Id);
        builder.Property(esr => esr.Id).ValueGeneratedOnAdd();
        
        builder.Property(esr => esr.Id).HasColumnName("id");
        builder.Property(esr => esr.SubjectSessionRewardId).HasColumnName("subject_session_reward_id");
        builder.Property(esr => esr.EmployeeSnapshotId).HasColumnName("employee_id");
        
        builder.HasOne(esr => esr.SubjectSessionReward)
            .WithMany(ssr => ssr.Employees)
            .HasForeignKey(esr => esr.SubjectSessionRewardId);
    }
}