using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Users;

namespace RewardFlow.TestUtilities.DataGenerators;

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