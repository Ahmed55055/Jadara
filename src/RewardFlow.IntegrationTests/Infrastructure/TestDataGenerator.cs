using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class TestDataGenerator
{
    /// <summary>
    /// Gets a generator for creating test user data.
    /// </summary>
    public static UserGenerator User => new();

    /// <summary>
    /// Gets a generator for creating test employee data.
    /// </summary>
    public static EmployeeFaker Employee => new();
}