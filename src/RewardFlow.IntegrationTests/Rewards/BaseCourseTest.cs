using RewardFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace RewardFlow.IntegrationTests.Rewards;

public class BaseCourseTest: IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;
    protected UserClient _userClient;

    public BaseCourseTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }
    
}