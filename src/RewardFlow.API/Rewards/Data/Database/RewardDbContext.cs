using Microsoft.EntityFrameworkCore;

namespace Reward_Flow_v2.Rewards.Data.Database;

public sealed class RewardDbContext(DbContextOptions<RewardDbContext> options) : DbContext(options)
{
    private const string Schema = "dbo";
    
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

    }
}