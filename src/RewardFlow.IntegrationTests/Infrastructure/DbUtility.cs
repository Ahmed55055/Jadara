using Microsoft.EntityFrameworkCore;
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
    private TestWebApplicationFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbUtility"/> class.
    /// </summary>
    /// <param name="factory">The test web application factory used to create service scopes.</param>
    public DbUtility(TestWebApplicationFactory factory)
    {
        _factory = factory;
        var scope = factory.Services.CreateScope();

        _contexts = new DbContext[]
        {
            scope.ServiceProvider.GetRequiredService<UserDbContext>(),
            scope.ServiceProvider.GetRequiredService<EmployeeDbContext>(),
            scope.ServiceProvider.GetRequiredService<RewardDbContext>()
        };
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

    public DbSet<T> Set<T>() where T : class
        => GetContext<T>().Set<T>();

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
    }
}