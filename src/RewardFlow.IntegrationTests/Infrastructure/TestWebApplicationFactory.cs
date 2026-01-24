using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
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
using Respawn;
using Respawn.Graph;
using RewardFlow.IntegrationTests.Auth;
using RewardFlow.IntegrationTests.Auth.Common;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test123!@#")
        .WithReuse(true)
        .WithName("dev-sql-server-reusable")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithPortBinding(10434, 1433)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Test123!@#",
                "-C", "-Q", "SELECT 1")
            .UntilInternalTcpPortIsAvailable(1433))
        .Build();

    private static Respawner _respawner = null!;
    private static string _connectionString;
    private static bool _isInitialized;
    
    public IConfiguration Configuration;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing database contexts
            var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<EmployeeDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<RewardDbContext>) ||
                    d.ServiceType == typeof(IDbContextFactory<RewardDbContext>)||
                    d.ServiceType == typeof(IResetPasswordMessageSender))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add test database contexts
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(_connectionString, o => o.CommandTimeout(120)));

            services.AddDbContext<EmployeeDbContext>(options =>
                options.UseSqlServer(_connectionString, o => o.CommandTimeout(120)));

            services.AddDbContext<RewardDbContext>(options =>
                options.UseSqlServer(_connectionString, o => o.CommandTimeout(120)));

            services.AddDbContextFactory<RewardDbContext>(options =>
                options.UseSqlServer(_connectionString, o => o.CommandTimeout(120)));

            services.AddScoped<IResetPasswordMessageSender,SpyEmailSender>();
        });
        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;
        
        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString() + ";Initial Catalog=RewardFlow";

        using var scope = Services.CreateScope();

        var contexts = new DbContext[]
        {
            scope.ServiceProvider.GetRequiredService<UserDbContext>(),
            scope.ServiceProvider.GetRequiredService<EmployeeDbContext>(),
            scope.ServiceProvider.GetRequiredService<RewardDbContext>()
        };

        // TODO:
        // Needs code refactor.
        // Context:
        // this code is tech-debt and will be listed as an issue to not delay delivery 
        // THE PROBLEM CONTEXT: The Container starts and database respond for the health check,
        // but it is not fully loaded so GetPendingMigrationsAsync returns some migrations to apply
        // while the database is up to date but needs some time.
        // Why 15 seconds? this is just the safe spot for my machine.
        // if encountered an exception in the future says "there is a database with same name"
        // then you are in the right place look at the code below as the database isn't ready when running GetPendingMigrationsAsync()
        await Task.Delay(TimeSpan.FromSeconds(15));

        foreach (var context in contexts)
        {
            var hasPendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (hasPendingMigrations.Any())
                await context.Database.MigrateAsync();
        }

        Configuration = Services.GetService<IConfiguration>()!;

        await InitializeRespawnerAsync();
        _isInitialized = true;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var conn =  new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    private async Task InitializeRespawnerAsync()
    {
        using var conn =  new SqlConnection(_connectionString);
        await conn.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(conn,
            new RespawnerOptions
            {
                SchemasToInclude = ["dbo"],
                TablesToIgnore = new Table[]
                {
                    "Role", "faculties", "departments", "plans", "employee_status", "job_titles","__EFMigrationsHistory"
                },
                DbAdapter = DbAdapter.SqlServer
            });
    }
}