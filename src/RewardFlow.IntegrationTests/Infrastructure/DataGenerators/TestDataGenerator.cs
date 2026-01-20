using RewardFlow.IntegrationTests.Infrastructure.DataGenerators.Fakers.Employees;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators.Fakers.Users;

namespace RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

public class TestDataGenerator
{
    /// <summary>
    /// Gets a generator for creating test user data.
    /// </summary>
    public static UserFaker User => new();

    /// <summary>
    /// Gets a generator for creating test employee data.
    /// </summary>
    public static EmployeeFaker Employee => new();
}