using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class CourseAssignmentConfiguration : IEntityTypeConfiguration<CourseAssignment>
{
    public void Configure(EntityTypeBuilder<CourseAssignment> builder)
    {
        builder.ToTable("course_assignments");
        builder.HasKey(ssr => ssr.Id);

        builder.Property(ssr => ssr.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");
        
        builder.Property(ssr => ssr.SessionRewardId)
            .HasColumnName("session_reward_id");
        
        builder.Property(ssr => ssr.TermCourseId)
            .HasColumnName("term_course_id");
        
        builder.Property(ssr => ssr.CourseSnapshotId)
            .HasColumnName("course_snapshot_id");
        
        builder.Property(ssr => ssr.StudentCount)
            .HasColumnName("number_of_students");
        
        builder.Property(ssr => ssr.SessionCount)
            .HasColumnName("number_of_sessions");
        
        builder.Property(ssr => ssr.MainEmployeeId)
            .HasColumnName("main_employee_id");
        
        builder.Property(ssr => ssr.MaxNumberOfEmployees)
            .HasColumnName("max_number_of_employees")
            .HasColumnType("smallint");

        builder.HasMany(ssr => ssr.StaffMembers)
            .WithOne(ess => ess.Course)
            .HasForeignKey(ess => ess.SubjectSessionRewardId);

        builder.HasOne<TermCourse>()
            .WithMany()
            .HasForeignKey(ssr => ssr.TermCourseId)
            .IsRequired();

        builder.HasOne<CourseSnapshot>()
            .WithMany()
            .HasForeignKey(ssr => ssr.CourseSnapshotId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}