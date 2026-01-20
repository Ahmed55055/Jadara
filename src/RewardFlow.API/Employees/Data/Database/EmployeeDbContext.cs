using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Reward_Flow_v2.Employees.Data.Database;

public sealed class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options, IConfiguration configuration) : DbContext(options)
{
    private const string Schema = "dbo";
    
    public DbSet<Employee> Employee => Set<Employee>();
    public DbSet<Department> Department => Set<Department>();
    public DbSet<EmployeeStatus> EmployeeStatus => Set<EmployeeStatus>();
    public DbSet<Faculty> Faculty => Set<Faculty>();
    public DbSet<JobTitle> JobTitle => Set<JobTitle>();
    public DbSet<EmployeeNameToken> EmployeeNameToken => Set<EmployeeNameToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema: Schema);
        modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeStatusEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FacultyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new JobTitleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeNameTokenEntityConfiguration());

       modelBuilder.Entity<EmployeeStatus>().HasData(
            new EmployeeStatus { StatusId = 1, Name = "Active", Description = "Active employee" },
            new EmployeeStatus { StatusId = 2, Name = "Inactive", Description = "Inactive employee" },
            new EmployeeStatus { StatusId = 3, Name = "Suspended", Description = "Suspended employee" }
        );

        modelBuilder.Entity<JobTitle>().HasData(
            new JobTitle { JobTitleId = 1, Name = "Employees", Description = "Regular employee" },
            new JobTitle { JobTitleId = 2, Name = "Professor", Description = "Professor" },
            new JobTitle { JobTitleId = 3, Name = "Teaching Assistant / Graduate Assistant", Description = "Teaching Assistant or Graduate Assistant" }
        );
    }
}