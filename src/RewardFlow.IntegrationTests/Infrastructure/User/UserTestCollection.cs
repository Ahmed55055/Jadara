using Xunit;

namespace RewardFlow.IntegrationTests.Infrastructure;
[CollectionDefinition("UserTests")]
public class UserTestCollection : ICollectionFixture<UserTestFixture>;