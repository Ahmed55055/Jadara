using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class CourseSnapshotConfiguration : IEntityTypeConfiguration<CourseSnapshot>
{
    public void Configure(EntityTypeBuilder<CourseSnapshot> builder)
    {
        builder.ToTable("course_snapshots");
        builder.HasKey(ss => ss.SnapshotId);

        builder.Property(ss => ss.SnapshotId)
            .HasColumnName("snapshot_id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ss => ss.CapturedAt)
            .HasColumnName("captured_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(ss => ss.CourseId)
            .HasColumnName("course_id");
        
        builder.Property(ss => ss.TermCourseId)
            .HasColumnName("term_course_id");
        
        builder.Property(ss => ss.CourseName)
            .HasColumnName("course_name")
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ss => ss.IsTheoretical)
            .HasColumnName("is_theoretical")
            .HasColumnType("bit");
        
        builder.Property(ss => ss.IsPractical)
            .HasColumnName("is_practical")
            .HasColumnType("bit");
        
        builder.Property(ss => ss.Term)
            .HasColumnName("term");
        
        builder.Property(ss => ss.Year)
            .HasColumnName("year");

        builder.HasOne(ss => ss.TermCourse)
            .WithMany()
            .HasForeignKey(ss => ss.TermCourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(c => c.CourseId);
    }
}