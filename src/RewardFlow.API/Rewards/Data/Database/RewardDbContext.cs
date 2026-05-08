using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reward_Flow_v2.Common.Extentions;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Rewards.Data.Database;

public sealed class RewardDbContext(DbContextOptions<RewardDbContext> options,  IConfiguration configuration, IUserContext userContext)
    : DbContext(options)
{
    private const string Schema = "DbReward";

    public DbSet<Subject> Subject => Set<Subject>();
    public DbSet<RewardEntity> Reward => Set<RewardEntity>();
    public DbSet<SemesterSubject> SubjectSemester => Set<SemesterSubject>();

    public DbSet<SessionRewardEntity> SessionRewardEntity => Set<SessionRewardEntity>();
    public DbSet<EmployeeSessionRewardEntity> EmployeeSessionRewardEntity => Set<EmployeeSessionRewardEntity>();
    public DbSet<SubjectSessionRewardEntity> SubjectSessionRewardEntity => Set<SubjectSessionRewardEntity>();
    public DbSet<EmployeeReward> EmployeeReward => Set<EmployeeReward>();
    public DbSet<EmployeeSnapshot> EmployeeSnapshots => Set<EmployeeSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema: Schema);
        modelBuilder.ApplyConfiguration(new SubjectEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RewardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SubjectSemesterEntityConfiguration());

        modelBuilder.ApplyConfiguration(new SessionRewardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SubjectSessionRewardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSessionRewardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeRewardConfiguration());

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