
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Reward_Flow_v2.User.Data;

namespace Reward_Flow_v2.User.Data.Database;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UUID)
            .HasColumnName("user_id")
            .HasDefaultValueSql("NEWID()");

        builder.HasIndex(u => u.UUID)
            .IsUnique();
        
        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(u => u.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.LastVisit)
            .HasColumnName("last_visit")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.ProfilePictureUrl)
            .HasColumnName("profile_picture_url")
            .HasMaxLength(255)
            .HasDefaultValue(null);

        builder.Property(u => u.PlanId)
            .HasColumnName("plan_id")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.RefreshToken)
            .HasColumnName("refresh_token")
            .HasMaxLength(500);

        builder.Property(u => u.RefreshTokenExpiry)
            .HasColumnName("refresh_token_expiry");

        builder.HasOne(u => u.UserRole)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .HasConstraintName("FK_Users_UserRoles");

        builder.HasOne(u => u.Plan)
            .WithMany()
            .HasForeignKey(u => u.PlanId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_users_plans_plan_id");

        builder.HasIndex(u => u.Username)
            .IsUnique();
    }

}
