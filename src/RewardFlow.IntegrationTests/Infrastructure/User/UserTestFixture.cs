using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.User.Data;
using Xunit;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class UserTestFixture : TestWebApplicationFactory, IAsyncLifetime
{
    private DbUtility dbUtility;

    public async Task InitializeAsync()
    {
        await base.InitializeAsync();

        dbUtility = new DbUtility(this);

        // No initial data needed for user tests
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        await base.DisposeAsync();
    }
}