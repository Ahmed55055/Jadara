using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.Data;
using RewardFlow_API.User.Data.Dtos;
using RewardFlow.TestUtilities.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Users;

/// <summary>
/// Integration tests for GetUserByUsername endpoint.
/// Tests various scenarios for retrieving user data by username including authorization checks.
/// </summary>
[Collection("UserTests")]
public class GetUserByUsernameTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly DbUtility _dbUtility;
    private readonly Faker _faker = new();
    private UserClient _adminClient;
    private UserClient _regularClient;
    private User _adminUser;
    private User _regularUser;
    private User _otherUser;

    public GetUserByUsernameTests(UserTestFixture factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }

    public async Task InitializeAsync()
    {
        _adminClient = await CreateUserWithRole(UserRoleEnum.Admin);
        _regularClient = await CreateUserWithRole(UserRoleEnum.User);
        var otherClient = await CreateUserWithRole(UserRoleEnum.User);
        _otherUser = otherClient.User;
    }

    private async Task<UserClient> CreateUserWithRole(UserRoleEnum role)
    {
        var user = TestDataGenerator.User.Generate();
        user.RoleId = (int)role;
        await _dbUtility.InsertAsync(user);
        return new UserClient(_factory, user);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUserByUsername_Admin_ShouldReturnUser()
    {
        // Act
        var response = await _adminClient.Client.GetAsync(AuthApiPath.GetUserByUsername.Replace("{username}", _regularClient.User.Username));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Username.Should().Be(_regularClient.User.Username);
        userDto.Email.Should().Be(_regularClient.User.Email);
    }

    [Fact]
    public async Task GetUserByUsername_RegularUser_OwnData_ShouldReturnUser()
    {
        // Act
        var response = await _regularClient.Client.GetAsync(AuthApiPath.GetUserByUsername.Replace("{username}", _regularClient.User.Username));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Username.Should().Be(_regularClient.User.Username);
        userDto.Email.Should().Be(_regularClient.User.Email);
    }

    [Fact]
    public async Task GetUserByUsername_RegularUser_OtherData_ShouldReturnForbidden()
    {
        // Act
        var response = await _regularClient.Client.GetAsync(AuthApiPath.GetUserByUsername.Replace("{username}", _otherUser.Username));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserByUsername_NoToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(AuthApiPath.GetUserByUsername.Replace("{username}", _regularClient.User.Username));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserByUsername_NonExistent_ShouldReturnNotFound()
    {
        // Act
        var response = await _adminClient.Client.GetAsync(AuthApiPath.GetUserByUsername.Replace("{username}", "nonexistentuser"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}