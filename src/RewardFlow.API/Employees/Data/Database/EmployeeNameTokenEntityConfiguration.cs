using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reward_Flow_v2.Employees.Data.Database;

public class EmployeeNameTokenEntityConfiguration : IEntityTypeConfiguration<EmployeeNameToken>
{
    public void Configure(EntityTypeBuilder<EmployeeNameToken> builder)
    {
        builder.ToTable("employee_name_tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.TokenHashed).HasColumnName("token_hashed").HasMaxLength(64);
        builder.Property(t => t.N).HasColumnName("n");
        builder.Property(t => t.EmployeeId).HasColumnName("employee_id");

        builder.HasIndex(t => new { t.TenantId, t.TokenHashed });
    }
}