using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");
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
