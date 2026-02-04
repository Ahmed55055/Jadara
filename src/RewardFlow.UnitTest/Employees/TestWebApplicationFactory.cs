using Microsoft.AspNetCore.Mvc.Testing;
using Reward_Flow_v2;

namespace RewardFlow_UnitTest.Employees;

public class TestWebApplicationFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}