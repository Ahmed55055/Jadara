using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Infrastructure.DataGenerators;
using RewardFlow.IntegrationTests.Auth.Common;
using Reward_Flow_v2.User.AuthService.Login;
using Reward_Flow_v2.User.AuthService.Register;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the Login endpoint.
/// Tests various scenarios for user login including validation, authentication, and error handling.
/// </summary>
[Collection("UserTests")]
public class LoginTests : IAsyncLifetime
{
    private readonly Faker _faker = new();
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client;
    private Reward_Flow_v2.User.Data.User _testUser;
    private string _testPassword;

    public LoginTests(UserTestFixture factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        // Register a test user
        _testUser = TestDataGenerator.User.Generate();
        _testPassword = _faker.Internet.Password();
        var registerRequest = RequestCreator.CreateRegisterRequest(_testUser, _testPassword);
        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/Register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var request = RequestCreator.CreateLoginRequest(_testUser, _testPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Login.Response>();
        result.Should().NotBeNull();
        result.JWTToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new Login.Request("nonexistentuser", _testPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = RequestCreator.CreateLoginRequest(_testUser, "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new Login.Request("", _testPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = RequestCreator.CreateLoginRequest(_testUser, "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}