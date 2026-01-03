using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reward_Flow_v2.Employees.Data.Database;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.User.Data.Database;
using Testcontainers.MsSql;
using Reward_Flow_v2;
using Xunit;
using RewardFlow.IntegrationTests.Infrastructure;
using System.Data.Common;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test123!@#")
        .WithReuse(true)
        .WithName("dev-sql-server-reusable")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithPortBinding(10434, 1433)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Test123!@#", "-C", "-Q", "SELECT 1")
            .UntilInternalTcpPortIsAvailable(1433))
        .Build();

    public IConfiguration _configuration;
    public DbConnection _connection;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing database contexts
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<EmployeeDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<RewardDbContext>) ||
                d.ServiceType == typeof(IDbContextFactory<RewardDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add test database contexts
            var connectionString = _dbContainer.GetConnectionString()+";Initial Catalog=RewardFlow";
            
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(connectionString, o => o.CommandTimeout(120)));
            
            services.AddDbContext<EmployeeDbContext>(options =>
                options.UseSqlServer(connectionString, o => o.CommandTimeout(120)));
            
            services.AddDbContext<RewardDbContext>(options =>
                options.UseSqlServer(connectionString, o => o.CommandTimeout(120)));
            
            services.AddDbContextFactory<RewardDbContext>(options =>
                options.UseSqlServer(connectionString, o => o.CommandTimeout(120)));

            // Replace authentication with test authentication
           
            
        });
        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Run migrations for all contexts
        using var scope = Services.CreateScope();
        
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var employeeDbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
        var rewardDbContext = scope.ServiceProvider.GetRequiredService<RewardDbContext>();

        // Apply migrations
        await userDbContext.Database.MigrateAsync();
        await employeeDbContext.Database.MigrateAsync();
        await rewardDbContext.Database.MigrateAsync();
        
        _configuration = Services.GetService<IConfiguration>();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var employeeDbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        
        // Clear all employee data but keep seed data
        employeeDbContext.Employee.RemoveRange(employeeDbContext.Employee);
        await employeeDbContext.SaveChangesAsync();
        
        // Clear test users but keep the main test user
        var testUsersToRemove = userDbContext.User
            .Where(u => u.UUID != TestAuthenticationHandler.TestUserGuid)
            .ToList();
        userDbContext.User.RemoveRange(testUsersToRemove);
        await userDbContext.SaveChangesAsync();
    }
}