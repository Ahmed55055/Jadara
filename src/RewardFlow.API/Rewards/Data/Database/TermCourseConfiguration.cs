using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class TermCourseConfiguration : IEntityTypeConfiguration<TermCourse>
{
    public void Configure(EntityTypeBuilder<TermCourse> builder)
    {
        builder.ToTable("term_course");
        builder.HasKey(ss => ss.Id);
        
        builder.Property(ss => ss.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ss => ss.CourseId)
            .HasColumnName("course_id")
            .IsRequired();
        
        builder.Property(ss => ss.Term)
            .HasColumnName("term")
            .IsRequired();
        
        builder.Property(ss => ss.StudentCount)
            .HasColumnName("number_of_students");
        
        builder.Property(ss=> ss.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.HasOne(ss => ss.Course)
            .WithMany(s => s.TermCourse)
            .OnDelete(DeleteBehavior.Restrict)
            .HasForeignKey(ss => ss.CourseId);
    }
}