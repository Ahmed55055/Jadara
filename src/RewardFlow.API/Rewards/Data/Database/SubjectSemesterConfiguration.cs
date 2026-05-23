using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SubjectSemesterConfiguration : IEntityTypeConfiguration<SemesterSubject>
{
    public void Configure(EntityTypeBuilder<SemesterSubject> builder)
    {
        builder.ToTable("subject_semesters");
        builder.HasKey(ss => ss.Id);
        
        builder.Property(ss => ss.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ss => ss.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();
        
        builder.Property(ss => ss.Semester)
            .HasColumnName("semester_number")
            .IsRequired();
        
        builder.Property(ss => ss.NumberOfStudents)
            .HasColumnName("number_of_students");
        
        builder.Property(ss=> ss.Year)
            .HasColumnName("year")
            .HasColumnType("smallint")
            .IsRequired();

        builder.HasOne(ss => ss.Subject)
            .WithMany(s => s.SubjectSemesters)
            .HasForeignKey(ss => ss.SubjectId);
    }
}