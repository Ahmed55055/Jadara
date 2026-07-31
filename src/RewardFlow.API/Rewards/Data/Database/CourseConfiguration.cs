using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;
using RewardFlow_API.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Id)
            .HasColumnName("id");
        
        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        
        builder.Property(s => s.IsTheoretical)
            .HasColumnName("is_theoretical")
            .HasColumnType("bit");
        
        builder.Property(s => s.IsPractical)
            .HasColumnName("is_practical")
            .HasColumnType("bit");
        
        builder.Property(s => s.SubjectPrice)
            .HasColumnName("subject_price")
            .HasColumnType("decimal(9,2)");
    }
}
