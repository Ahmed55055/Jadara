using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SubjectSnapshotConfiguration : IEntityTypeConfiguration<CourseSnapshot>
{
    public void Configure(EntityTypeBuilder<CourseSnapshot> builder)
    {
        builder.ToTable("subject_snapshots");
        builder.HasKey(ss => ss.SnapshotId);

        builder.Property(ss => ss.SnapshotId)
            .HasColumnName("snapshot_id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ss => ss.CapturedAt)
            .HasColumnName("captured_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(ss => ss.SemesterSubjectId)
            .HasColumnName("semester_subject_id");
        
        builder.Property(ss => ss.SubjectName)
            .HasColumnName("subject_name")
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ss => ss.IsTheoretical)
            .HasColumnName("is_theoretical")
            .HasColumnType("bit");
        
        builder.Property(ss => ss.IsPractical)
            .HasColumnName("is_practical")
            .HasColumnType("bit");
        
        builder.Property(ss => ss.Semester)
            .HasColumnName("semester");
        
        builder.Property(ss => ss.Year)
            .HasColumnName("year");

        builder.HasOne(ss => ss.TermCourse)
            .WithMany()
            .HasForeignKey(ss => ss.SemesterSubjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}