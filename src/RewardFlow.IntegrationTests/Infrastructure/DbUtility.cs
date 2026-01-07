using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reward_Flow_v2.Employees.Data.Database;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.User.Data.Database;
using System.Collections.Immutable;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class DbUtility: IDisposable
{
    private readonly DbContext[] _contexts;

    public DbUtility(TestWebApplicationFactory factory)
    {
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
        var context = _contexts.FirstOrDefault(c => c.Model.FindEntityType(typeof(T)) is not null)
                      ?? throw new ArgumentException($"Context {typeof(T)} does not exist");

        return context;
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

    public async Task<bool> AnyAsync<T>() where T : class
    {
        var context = GetContext<T>();
        return await context.Set<T>().AnyAsync();
    }
    
    public async Task<List<T>> GetAllAsync<T>() where T : class
    {
        var context = GetContext<T>();
        return await context.Set<T>().ToListAsync();
    }
    
    
    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
    }
}