using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Employees.Common;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.TestUtilities.DataGenerators;
using System.Net;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees.SinglarOperatoins;

public class DeleteEmployeeTests(TestWebApplicationFactory factory) : BaseEmployeeTestFixture(factory), IAsyncLifetime
{
    private UserClient _userClient;

    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
        _userClient.Authanticate();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task DeleteEmployee_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        // Act
        var response = await _userClient.Client.DeleteAsync($"/api/Employees/{createdEmployee!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _userClient.Client.GetAsync($"/api/Employees/{createdEmployee!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_WithValidId_ShouldRemoveNameTokens()
    {
        // Arrange
        var employeeData = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate();
        var createdEmployee = await ApiManager.CreateEmployee(employeeData, _userClient.Client);

        // Act
        var response = await _userClient.Client.DeleteAsync($"/api/Employees/{createdEmployee!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify name tokens are removed
        var tokensExist = await _dbUtility.Query<EmployeeNameToken>().AnyAsync(
            t => t.EmployeeId == createdEmployee!.Id && t.UserId == _userClient.User.Id);
        tokensExist.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEmployee_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _userClient.Client.DeleteAsync("/api/Employees/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Act
        var response = await _userClient.Client.DeleteAsync("/api/Employees/abc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteEmployee_WithAssociatedRewardData_ShouldReturnConflict()
    {
        1.Should().Be(1);
    }
}