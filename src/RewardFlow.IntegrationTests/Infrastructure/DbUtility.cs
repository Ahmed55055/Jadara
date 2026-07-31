using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reward_Flow_v2.Employees.Data.Database;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.User.Data.Database;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace RewardFlow.IntegrationTests.Infrastructure;

/// <summary>
/// Utility class for managing database operations in integration tests.
/// </summary>
public class DbUtility : IDisposable
{
    private readonly DbContext[] _contexts;
    private readonly string _connectionString;
    private TestWebApplicationFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbUtility"/> class.
    /// </summary>
    /// <param name="factory">The test web application factory used to create service scopes.</param>
    public DbUtility(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _connectionString = TestWebApplicationFactory.ConnectionString;
        var options = 
        _contexts =
        [
            new UserDbContext(new DbContextOptionsBuilder<UserDbContext>()
                .UseSqlServer(_connectionString)
                .Options, factory.Configuration),

            new EmployeeDbContext(
                new DbContextOptionsBuilder<EmployeeDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options, factory.Configuration, null),

            new RewardDbContext(
                new DbContextOptionsBuilder<RewardDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options, factory.Configuration, null)
            /*CreateContext<UserDbContext>(),
            CreateContext<EmployeeDbContext>(),
            CreateContext<RewardDbContext>()*/
        ];
    }
    
    public DbContext GetContext<T>() where T : class
    {
        var scope = _factory.Services.CreateScope();

        var contextType = _contexts
                              .FirstOrDefault(c => c.Model.FindEntityType(typeof(T)) is not null)?
                              .GetType()
                          ?? throw new ArgumentException($"No context found that manages entity type {typeof(T).Name}");

        return (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
    }

    public async Task InsertAsync<T>(T entity) where T : class
    {
        var context = GetContext<T>();

        context.Set<T>().Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task InsertRangeAsync<T>(IEnumerable<T> entities) where T : class
    {
        var context = GetContext<T>();

        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync();
    }

    public IQueryable<T> Query<T>() where T : class
        => GetContext<T>().Set<T>().IgnoreQueryFilters();

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
    }
}