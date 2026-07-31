using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SemesterSubjectConfiguration : IEntityTypeConfiguration<TermCourse>
{
    public void Configure(EntityTypeBuilder<TermCourse> builder)
    {
        builder.ToTable("subject_semesters");
        builder.HasKey(ss => ss.Id);
        
        builder.Property(ss => ss.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");
        
        builder.Property(ss => ss.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();
        
        builder.Property(ss => ss.Semester)
            .HasColumnName("semester_number")
            .IsRequired();
        
        builder.Property(ss => ss.NumberOfStudents)
            .HasColumnName("number_of_students");
        
        builder.Property(ss => ss.Price)
            .HasColumnName("price")
            .IsRequired(false);
        
        builder.Property(ss => ss.Year)
            .HasColumnName("year")
            .HasColumnType("smallint")
            .IsRequired();

        builder.HasOne(ss => ss.Course)
            .WithMany(s => s.SubjectSemesters)
            .HasForeignKey(ss => ss.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
