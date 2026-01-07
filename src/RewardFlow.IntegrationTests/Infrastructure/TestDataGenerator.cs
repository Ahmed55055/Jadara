using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class TestDataGenerator
{
    public static UserGenerator User => new();
    public static EmployeeFaker Employee => new();
}