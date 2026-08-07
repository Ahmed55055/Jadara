using Bogus;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.User.Data;
using RewardFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

public class BaseAuthTestFixture: IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;
    protected readonly Faker _faker = new();


    public BaseAuthTestFixture(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }

    public async Task InitializeAsync() => await Task.CompletedTask;
}