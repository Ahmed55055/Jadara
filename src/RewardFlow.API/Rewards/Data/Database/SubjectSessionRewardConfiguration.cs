using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SubjectSessionRewardConfiguration : IEntityTypeConfiguration<SubjectSessionRewardEntity>
{
    public void Configure(EntityTypeBuilder<SubjectSessionRewardEntity> builder)
    {
        builder.ToTable("subject_session_rewards");
        builder.HasKey(ssr => ssr.Id);

        builder.Property(ssr => ssr.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");
        
        builder.Property(ssr => ssr.SessionRewardId)
            .HasColumnName("session_reward_id");
        
        builder.Property(ssr => ssr.SemesterSubjectId)
            .HasColumnName("semester_subject_id");
        
        builder.Property(ssr => ssr.SubjectSnapshotId)
            .HasColumnName("subject_snapshot_id");
        
        builder.Property(ssr => ssr.NumberOfStudents)
            .HasColumnName("number_of_students");
        
        builder.Property(ssr => ssr.SessionCount)
            .HasColumnName("number_of_sessions");
        
        builder.Property(ssr => ssr.MainEmployeeId)
            .HasColumnName("main_employee_id");
        
        builder.Property(ssr => ssr.MaxNumberOfEmployees)
            .HasColumnName("max_number_of_employees")
            .HasColumnType("smallint");

        builder.HasMany(ssr => ssr.EmployeeSessionSubject)
            .WithOne(ess => ess.SubjectSessionReward)
            .HasForeignKey(ess => ess.SubjectSessionRewardId);

        builder.HasOne<SemesterSubject>()
            .WithMany()
            .HasForeignKey(ssr => ssr.SemesterSubjectId)
            .IsRequired();

        builder.HasOne<SubjectSnapshot>()
            .WithMany()
            .HasForeignKey(ssr => ssr.SubjectSnapshotId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}