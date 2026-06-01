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
        .WithDockerEndpoint("http://192.168.1.107:2375")
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test123!@#")
        .WithReuse(true)
        .WithName("dev-sql-server-reusable")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithPortBinding(1434, 1433)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Test123!@#",
                "-C", "-Q", "SELECT 1")
            .UntilInternalTcpPortIsAvailable(1433))
        .Build();

    private static Respawner _respawner = null!;
    private static string _connectionString;
    private static bool _isInitialized = false;

    public IConfiguration Configuration { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing database contexts
            var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<EmployeeDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<RewardDbContext>) ||
                    d.ServiceType == typeof(IDbContextFactory<RewardDbContext>) ||
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

            services.AddScoped<IResetPasswordMessageSender, SpyEmailSender>();
        });
        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        Configuration = Services.GetService<IConfiguration>();
        
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

        var isDatabaseReady = false;
        var retries = 0;

        while (!isDatabaseReady && retries < 200) // 3s * 200 = 10 min
        {
            try
            {
                await contexts[0].Database.ExecuteSqlRawAsync("SELECT DB_ID('RewardFlow')");
                isDatabaseReady = true;            }
            catch
            {
                retries++;
                await Task.Delay(3000);
            }
        }

        foreach (var context in contexts)
        {
            var hasPendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (hasPendingMigrations.Any())
                await context.Database.MigrateAsync();
        }
        
        await InitializeRespawnerAsync();
        _isInitialized = true;
    }

    public new async Task DisposeAsync()
    {
        //await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        // using var conn = new SqlConnection(_connectionString);
        // await conn.OpenAsync();
        // await _respawner.ResetAsync(conn);
    }

    private async Task InitializeRespawnerAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn,
            new RespawnerOptions
            {
                SchemasToInclude = ["dbo"],
                TablesToIgnore = new Table[]
                {
                    "Role", "faculties", "departments", "plans", "employee_status", "job_titles",
                    "__EFMigrationsHistory"
                },
                DbAdapter = DbAdapter.SqlServer
            });
    }
}