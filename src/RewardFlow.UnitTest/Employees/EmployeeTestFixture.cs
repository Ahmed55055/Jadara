using Moq;
using Reward_Flow_v2.Employees.Data.Database;

namespace RewardFlow_UnitTest.Employees;

public class EmployeeTestFixture: TestWebApplicationFactory, IAsyncLifetime
{
    public Mock<EmployeeDbContext> EmployeeDbContextMock;
    
    public Task InitializeAsync()
    {
        base.InitializeAsync();
        throw new NotImplementedException();
    }

    public Task DisposeAsync()
    {
        base.DisposeAsync();
        throw new NotImplementedException();
    }
}