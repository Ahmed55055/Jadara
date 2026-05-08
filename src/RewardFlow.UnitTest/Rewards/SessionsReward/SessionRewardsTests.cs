using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory; 
using Moq;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using SessionEmployeeDto = Reward_Flow_v2.Rewards.SessionsReward.Dtos.EmployeeDto;
using LookupEmployeeDto = Reward_Flow_v2.Common.EmployeeLookup.EmployeeDto;
using EmployeeSalaryDto = Reward_Flow_v2.Common.EmployeeLookup.EmployeeSalaryDto;
using IEmployeeLookupService = Reward_Flow_v2.Common.EmployeeLookup.IEmployeeLookupService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RewardFlow_UnitTest.Rewards.SessionsReward
{
    public class SessionRewardsTests : IDisposable
    {
        private readonly DbContextOptions<RewardDbContext> _options;
        private readonly Mock<IDbContextFactory<RewardDbContext>> _mockDbContextFactory;
        private readonly Mock<ISessionRewardCalculator> _mockCalculator;
        private readonly Mock<ISessionRewardRules> _mockRules;
        private readonly Mock<IEmployeeLookupService> _mockEmployeeLookup;
        private readonly SessionRewards.SessionRewardFactory _factory;
        private const int CreatedBy = 1;

        public SessionRewardsTests()
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
            
            _factory = new SessionRewards.SessionRewardFactory(_mockCalculator.Object, _mockRules.Object, _mockDbContextFactory.Object, _mockEmployeeLookup.Object);
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
        public async Task UpdateTotal_WithEmployeeSessions_UpdatesEmployeeRewards()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);
            var sessionReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);
            
            using var context = new RewardDbContext(_options);
            await SeedTestData(context, sessionReward.RewardId, sessionReward.RewardId);
            
            _mockCalculator.Setup(x => x.CalculateTotal(10, 5000f, 0.1f)).Returns(5000f);
            _mockRules.Setup(x => x.IsWithInMaximumNumberOfSession(10)).Returns(true);
            _mockEmployeeLookup.Setup(x => x.GetEmployeesSalaryById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<EmployeeSalaryDto> { new() { EmployeeId = 1, Salary = 5000f } });

            // Act
            await sessionReward.UpdateTotal();

            // Assert
            var employeeReward = await sessionReward.GetEmployeeReward(1);
            employeeReward.Should().NotBeNull();
            employeeReward.Total.Should().Be(5000f);
        }

        [Fact]
        public async Task AssignEmployeeToSubjectAsync_NewAssignment_ReturnsTrue()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);
            var sessionReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);
            
            using var context = new RewardDbContext(_options);
            await SeedSubjectSemester(context);
            _mockCalculator.Setup(x => x.CalculateSessions(25)).Returns(5);

            _mockRules
                .Setup(x => x.CanAssignEmployeeToSubjectAsync(
                    It.Is<int>(n => n > 0),
                    It.Is<int>(n => n > 0),
                    It.Is<IEnumerable<int>>(ids => ids.All(i => i > 0))))
                .Returns((true));
            
            var dto = new SessionSubjectDto
            {
                SubjectId = 1,
                NumberOfStudents = 25,
                MainEmployeeId = 1,
                Employees = new List<SessionEmployeeDto> { new() { EmployeeId = 1, Salary = 5000f } }
            };

            // Act
            var result = await sessionReward.AssignEmployeeToSubjectAsync(dto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AssignEmployeeToSubjectAsync_InvalidNumberOfStudents_ReturnsFalse()
        {
            // Arrange
            var sessionRewardId = await _factory.CreateAsync(CreatedBy, "Test Reward", "TR001", 2024, 1, 0.1f);
            var sessionReward = await _factory.FindAsync(sessionRewardId.Value, CreatedBy);
            
            var dto = new SessionSubjectDto
            {
                SubjectId = 1,
                NumberOfStudents = 0,
                MainEmployeeId = 1,
                Employees = new List<SessionEmployeeDto> { new() { EmployeeId = 1, Salary = 5000f } }
            };

            // Act
            var result = await sessionReward.AssignEmployeeToSubjectAsync(dto);

            // Assert
            result.Should().BeFalse();
        }

        private async Task SeedTestData(RewardDbContext context, int sessionRewardId, int rewardId, int totalSessions = 10)
        {
            await SeedSubjectSemester(context);
            
            var subjectSessionReward = new SubjectSessionRewardEntity
            {
                SessionRewardId = sessionRewardId,
                NumberOfSessions = totalSessions,
                SemesterSubjectId = 1,
                StudentsNumber = 25,
                MainEmployeeId = 1,
                MaxNumberOfEmployees = 3
            };
            context.SubjectSessionRewardEntity.Add(subjectSessionReward);
            await context.SaveChangesAsync();
            
            context.EmployeeSessionRewardEntity.Add(new EmployeeSessionRewardEntity
            {
                EmployeeSnapshotId = 1,
                SubjectSessionRewardId = subjectSessionReward.Id
            });
            
            context.EmployeeReward.Add(new EmployeeReward
            {
                RewardId = rewardId,
                EmployeeId = 1,
                Total = 0,
                IsUpdated = false
            });
            
            await context.SaveChangesAsync();
        }
        
        private async Task SeedSubjectSemester(RewardDbContext context)
        {
            if (!context.SubjectSemester.Any())
            {
                context.SubjectSemester.AddRange(
                    new SemesterSubject { Id = 1, SubjectId = 1, SemesterNumber = 1, NumberOfStudents = 25 },
                    new SemesterSubject { Id = 2, SubjectId = 2, SemesterNumber = 1, NumberOfStudents = 30 }
                );
                await context.SaveChangesAsync();
            }
        }
        
        private async Task SeedLargeTestData(RewardDbContext context, int sessionRewardId, int rewardId, int employeeCount)
        {
            await SeedSubjectSemester(context);
            
            var subjectSessionReward = new SubjectSessionRewardEntity
            {
                SessionRewardId = sessionRewardId,
                NumberOfSessions = 10,
                SemesterSubjectId = 1,
                StudentsNumber = 25,
                MainEmployeeId = 1,
                MaxNumberOfEmployees = 3
            };
            context.SubjectSessionRewardEntity.Add(subjectSessionReward);
            await context.SaveChangesAsync();
            
            var employeeSessionRewards = Enumerable.Range(1, employeeCount)
                .Select(i => new EmployeeSessionRewardEntity
                {
                    EmployeeSnapshotId = i,
                    SubjectSessionRewardId = subjectSessionReward.Id
                });
            context.EmployeeSessionRewardEntity.AddRange(employeeSessionRewards);
            
            var employeeRewards = Enumerable.Range(1, employeeCount)
                .Select(i => new EmployeeReward
                {
                    RewardId = rewardId,
                    EmployeeId = i,
                    Total = 0,
                    IsUpdated = false
                });
            context.EmployeeReward.AddRange(employeeRewards);
            
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            // Context is created per test, no need to dispose
        }
    }
}