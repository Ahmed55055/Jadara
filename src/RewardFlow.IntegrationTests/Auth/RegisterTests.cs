using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Auth.Common;
using Reward_Flow_v2.User.AuthService.Register;
using Reward_Flow_v2.User.Data;
using RewardFlow.TestUtilities.DataGenerators;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the Register endpoint.
/// Tests various scenarios for user registration including validation, duplicate detection, and successful creation.
/// </summary>
public class RegisterTests(TestWebApplicationFactory factory) : BaseAuthTestFixture(factory), IAsyncLifetime
{
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        var password = _faker.Internet.Password();
        var request = RequestCreator.CreateRegisterRequest(userData, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Register.Response>();
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.JwtToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        // Assert user data matches
        result.User.Username.Should().Be(userData.Username);
        result.User.Email.Should().Be(userData.Email);
    }

    [Fact]
    public async Task Register_WithValidDataWithoutEmail_ShouldReturnCreated()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        userData.Email = null;
        var password = _faker.Internet.Password();
        var request = RequestCreator.CreateRegisterRequest(userData, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Register.Response>();
        result.Should().NotBeNull();
        result.User.Email.Should().BeNull();
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturnConflict()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        var password1 = _faker.Internet.Password();
        var request1 = RequestCreator.CreateRegisterRequest(userData, password1);
        await _client.PostAsJsonAsync("/api/Auth/Register", request1); // Create first

        var password2 = _faker.Internet.Password();
        var request2 = RequestCreator.CreateRegisterRequest(userData, password2);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        userData.Username = "";
        var password = _faker.Internet.Password();
        var request = RequestCreator.CreateRegisterRequest(userData, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        var password = "123";
        var request = RequestCreator.CreateRegisterRequest(userData, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userData = TestDataGenerator.User.Generate();
        userData.Email = "invalid-email";
        var password = _faker.Internet.Password();
        var request = RequestCreator.CreateRegisterRequest(userData, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldHashPassword()
    {
        // Arrange
        var users = TestDataGenerator.User.Generate(5);
        var password = "TestPassword123!";

        // Act
        foreach (var user in users)
        {
            var request = RequestCreator.CreateRegisterRequest(user, password);
            await _client.PostAsJsonAsync("/api/Auth/Register", request);
        }

        // Assert
        var hashes = new List<string>();
        foreach (var user in users)
        {
            var dbUser = await _dbUtility.Query<User>().FirstOrDefaultAsync(u => u.Username == user.Username);
            dbUser.Should().NotBeNull();
            dbUser.PasswordHash.Should().NotBe(password); // Should be hashed
            dbUser.PasswordHash.Should().NotBeNullOrEmpty();
            hashes.Add(dbUser.PasswordHash);
        }

        // Ensure all hashes are unique (salting)
        hashes.Should().OnlyHaveUniqueItems();
    }
}