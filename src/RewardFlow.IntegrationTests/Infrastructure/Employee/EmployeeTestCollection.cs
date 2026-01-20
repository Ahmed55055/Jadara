using Xunit;

namespace RewardFlow.IntegrationTests.Infrastructure;
[CollectionDefinition("EmployeeTests")]
public class EmployeeTestCollection : ICollectionFixture<EmployeeTestFixture>;
