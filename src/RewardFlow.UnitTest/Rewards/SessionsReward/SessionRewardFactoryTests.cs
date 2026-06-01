/*using Microsoft.EntityFrameworkCore;
using Moq;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Common;
using Xunit;
using FluentAssertions;

namespace RewardFlow_UnitTest.Rewards.SessionsReward
{
    public class SessionRewardFactoryTests : IDisposable
    {
        private readonly DbContextOptions<RewardDbContext> _options;
        private readonly Mock<IDbContextFactory<RewardDbContext>> _mockDbContextFactory;
        private readonly Mock<ISessionRewardCalculator> _mockCalculator;
        private readonly Mock<ISessionRewardRules> _mockRules;
        private readonly Mock<IEmployeeLookupService> _mockEmployeeLookup;
        private readonly SessionRewards.SessionRewardFactory _factory;
        private const int CreatedBy = 1;

        public SessionRewardFactoryTests()
        {
            _options = new DbContextOptionsBuilder<RewardDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            
            _mockDbContextFactory = new Mock<IDbContextFactory<RewardDbContext>>();
            _mockDbContextFactory.Setup(x => x.CreateDbContext()).Returns(() => new RewardDbContext(_options));

            _mockCalculator = new Mock<ISessionRewardCalculator>();
            _mockRules = new Mock<ISessionRewardRules>();
            _mockEmployeeLookup = new Mock<IEmployeeLookupService>();
            
            // _factory = new SessionRewards.SessionRewardFactory(_mockCalculator.Object, _mockRules.Object, _mockDbContextFactory.Object, _mockEmployeeLookup.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidData_ReturnsSessionRewardId()
        {
            // Act
            var result = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_NullCode_ReturnsSessionRewardId()
        {
            // Act
            var result = await _factory.CreateAsync(CreatedBy, "Test Reward", null, 2024, 1, 0.1f);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FindAsync_ExistingSessionReward_ReturnsSessionReward()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);

            // Act
            var result = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);

            // Assert
            result.Should().NotBeNull();
            result.SessionRewardId.Should().Be(sessionRewardId.Value);
        }

        [Fact]
        public async Task FindAsync_NonExistentSessionReward_ReturnsNull()
        {
            // Act
            var result = await _factory.FindAsync(999, CreatedBy);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindAsync_WrongCreatedBy_ReturnsNull()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);

            // Act
            var result = await _factory.FindAsync(sessionRewardId.Value, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindByRewardIdAsync_ExistingReward_ReturnsSessionReward()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);
            var sessionReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);

            // Act
            var result = await _factory.FindByRewardIdAsync(sessionReward.RewardId, CreatedBy);

            // Assert
            result.Should().NotBeNull();
            result.SessionRewardId.Should().Be(sessionReward.RewardId);
        }

        [Fact]
        public async Task FindByRewardIdAsync_NonExistentReward_ReturnsNull()
        {
            // Act
            var result = await _factory.FindByRewardIdAsync(999, CreatedBy);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ExistingSessionReward_UpdatesAllFields_ReturnsTrue()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Original Name", "ORIG", 2023, 1, 0.1f);

            // Act
            var result = await _factory.UpdateAsync(
                sessionRewardId.Value, 
                CreatedBy, 
                new Optional<string> { HasValue = true, Value = "Updated Name" },
                new Optional<string> { HasValue = true, Value = "UPD" },
                new Optional<int?> { HasValue = true, Value = 2024 },
                new Optional<byte?> { HasValue = true, Value = 2 },
                new Optional<decimal> { HasValue = true, Value = 0.2m }
            );

            // Assert
            result.Should().BeTrue();
            
            var updatedReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);
            updatedReward.Should().NotBeNull();
            updatedReward.Name.Should().Be("Updated Name");
            updatedReward.Code.Should().Be("UPD");
            updatedReward.Year.Should().Be(2024);
            updatedReward.Semester.Should().Be(2);
            updatedReward.Percentage.Should().Be(0.2m);
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_UpdatesOnlySpecifiedFields_ReturnsTrue()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Original Name", "ORIG", 2023, 1, 0.1f);

            // Act
            var result = await _factory.UpdateAsync(
                sessionRewardId.Value, 
                CreatedBy, 
                name: new Optional<string> { HasValue = true, Value = "Updated Name" },
                percentage: new Optional<decimal> { HasValue = true, Value = 0.3f }
            );

            // Assert
            result.Should().BeTrue();
            
            var updatedReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);
            updatedReward.Name.Should().Be("Updated Name");
            updatedReward.Code.Should().Be("ORIG"); // Should remain unchanged
            updatedReward.Year.Should().Be(2023); // Should remain unchanged
            updatedReward.Semester.Should().Be(1); // Should remain unchanged
            updatedReward.Percentage.Should().Be(0.3f);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentSessionReward_ReturnsFalse()
        {
            // Act
            var result = await _factory.UpdateAsync(999, CreatedBy, new Optional<string> { HasValue = true, Value = "Test" });

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_WrongCreatedBy_ReturnsFalse()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);

            // Act
            var result = await _factory.UpdateAsync(sessionRewardId.Value, 999, new Optional<string> { HasValue = true, Value = "Test" });

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_NoFieldsToUpdate_ReturnsTrue()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);

            // Act
            var result = await _factory.UpdateAsync(sessionRewardId.Value, CreatedBy);

            // Assert
            result.Should().BeTrue();
        }

        public void Dispose()
        {
            // Context is created per test, no need to dispose
        }
    }
}*/