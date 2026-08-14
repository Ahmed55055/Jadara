using Microsoft.EntityFrameworkCore;
using RewardFlow_API.Rewards.Common;
using RewardFlow_API.Rewards.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Common.Extentions;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data.Database;

public sealed class RewardDbContext(DbContextOptions<RewardDbContext> options,  IConfiguration configuration, IUserContext userContext)
    : DbContext(options)
{
    private const string Schema = "DbReward";
    
    // Rewards
    public DbSet<Reward> Reward => Set<Reward>();
    public DbSet<SessionRewardEntity> SessionRewardEntity => Set<SessionRewardEntity>();

    // Subjects
    public DbSet<Course> Course => Set<Course>();
    public DbSet<TermCourse> TermCourse => Set<TermCourse>();
    public DbSet<CourseSnapshot> CourseSnapshot => Set<CourseSnapshot>();
    
    // Sessions Reward
    public DbSet<EmployeeSessions> EmployeeSessions => Set<EmployeeSessions>();
    public DbSet<CourseEmployee> CourseEmployee => Set<CourseEmployee>();
    public DbSet<CourseAssignment> CourseAssignment => Set<CourseAssignment>();
    public DbSet<EmployeeReward> EmployeeReward => Set<EmployeeReward>();
    public DbSet<EmployeeSnapshot> EmployeeSnapshots => Set<EmployeeSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema: Schema);
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new RewardConfiguration());
        modelBuilder.ApplyConfiguration(new TermCourseConfiguration());

        modelBuilder.ApplyConfiguration(new SessionRewardConfiguration());
        modelBuilder.ApplyConfiguration(new CourseAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSessionRewardConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeRewardConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSessionSubjectConfiguration());
        modelBuilder.ApplyConfiguration(new CourseSnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSnapshotConfiguration());

        foreach (var entity in modelBuilder.GetEntityBuilders<ITenantEntity>())
        {
            entity.Property((ITenantEntity t) => t.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.HasIndex<ITenantEntity>(t => t.TenantId);

            entity.HasQueryFilter<ITenantEntity>(e => e.TenantId == userContext.GetTenantId());
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var currentTenantId = userContext.GetTenantId();

        var addedEntities = ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entityEntry in addedEntities)
        {
            entityEntry.Entity.TenantId = currentTenantId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}