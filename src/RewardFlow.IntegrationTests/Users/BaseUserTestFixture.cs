using Bogus;
using Microsoft.EntityFrameworkCore;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Common;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.TestUtilities.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Users;

/// <summary>
/// Shared base fixture for user integration tests
/// </summary>
public class BaseUserTestFixture : IClassFixture<TestWebApplicationFactory>,IAsyncLifetime
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;
    protected UserClient _userClient;
    protected readonly Faker _faker = new();

    public BaseUserTestFixture(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }

    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}