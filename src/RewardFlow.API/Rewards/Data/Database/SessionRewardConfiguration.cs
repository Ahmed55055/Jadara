using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Rewards.Data;

namespace Reward_Flow_v2.Rewards.Data.Database;

public class SessionRewardConfiguration : IEntityTypeConfiguration<SessionRewardEntity>
{
    public void Configure(EntityTypeBuilder<SessionRewardEntity> builder)
    {
        const string tableName = "session_rewards";
        
        builder.ToTable(tableName);
        builder.HasKey(sr => sr.Id);
        
        builder.Property(sr => sr.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("session_reward_id");
        
        builder.Property(sr => sr.year)
            .HasColumnName("year")
            .HasColumnType("smallint");
        
        builder.Property(sr => sr.semester)
            .HasColumnName("semester");

        builder.Property(sr => sr.Percentage)
            .HasColumnName("percentage")
            .HasColumnType("decimal(5,2)");
        
        builder.ToTable(tableName, t => 
            t.HasCheckConstraint("CK_Session_Reward_Percentage_Min", "[percentage] >= 0"));
        
        builder.HasOne(sr => sr.Reward)
            .WithMany()
            .HasForeignKey(sr => sr.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
