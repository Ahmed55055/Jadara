using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class EmployeeSessionSubjectConfiguration : IEntityTypeConfiguration<CourseEmployee>
{
    public void Configure(EntityTypeBuilder<CourseEmployee> builder)
    {
        builder.ToTable("employee_session_subjects");
        builder.HasKey(ess => new { ess.SubjectSessionRewardId, ess.EmployeeId });

        builder.Property(ess => ess.SubjectSessionRewardId)
            .HasColumnName("subject_session_reward_id")
            .IsRequired();

        builder.Property(ess => ess.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(ess => ess.EmployeeSnapshotId)
            .HasColumnName("employee_snapshot_id");

        builder.HasOne(ess => ess.Course)
            .WithMany()
            .HasForeignKey(ess => ess.SubjectSessionRewardId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ess => ess.EmployeeSnapshot)
            .WithMany()
            .HasForeignKey(ess => ess.EmployeeSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}