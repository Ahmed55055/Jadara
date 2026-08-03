using Xunit;

namespace RewardFlow.IntegrationTests.Infrastructure.Rewards;

public class SessionRewardFixture: TestWebApplicationFactory, IAsyncLifetime
{
    private DbUtility dbUtility;

    public async Task InitializeAsync()
    {
        await base.InitializeAsync();

        dbUtility = new DbUtility(this);
        
        
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        await base.DisposeAsync();
    }
}