using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Auth.Common;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.Data;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Users;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the Reset Password functionality.
/// Tests various scenarios for password reset including request, verification, confirmation, and error handling.
/// </summary>
[Collection("UserTests")]
public class ResetPasswordTests(UserTestFixture factory) : IAsyncLifetime
{
    private readonly Faker _faker = new();
    private HttpClient _client = null!;
    private User _testUser= null!;

    public async Task InitializeAsync()
    {
        _client = factory.CreateClient();
        
        // Register a test user
        _testUser = TestDataGenerator.User.WithValue(UserFields.Email).Generate();
        var password = _faker.Internet.Password();
        var registerRequest = RequestCreator.CreateRegisterRequest(_testUser,password);
        var registerResponse = await _client.PostAsJsonAsync(AuthApiPath.Register, registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResetPassword_FullFlow_ShouldSucceed()
    {
        string resetToken = await ResetRequest();

        await VerifyToken(resetToken);

        string newPassword = await ConfirmReset(resetToken);

        await VerifyCanLogin(newPassword);
    }

    private async Task VerifyCanLogin(string newPassword)
    {
        // Verify user can login with new password
        var loginRequest = RequestCreator.CreateLoginRequest(_testUser, newPassword);
        var loginResponse = await _client.PostAsJsonAsync(AuthApiPath.Login, loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> ConfirmReset(string resetToken)
    {
        // Act - Confirm reset
        var newPassword = GenerateValidPassword();
        var confirmResetRequest = RequestCreator.CreateConfirmResetPasswordRequest(resetToken, newPassword);
        var confirmResetResponse = await _client.PostAsJsonAsync(AuthApiPath.ConfirmResetPassword, confirmResetRequest);

        // Assert - Confirm reset succeeds
        confirmResetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmResetResult = await confirmResetResponse.Content.ReadFromJsonAsync<ConfirmResetPassword.Response>();
        confirmResetResult.Should().NotBeNull();
        confirmResetResult.IsSuccess.Should().BeTrue();
        return newPassword;
    }

    private async Task VerifyToken(string resetToken)
    {
        // Act - Verify token
        var verifyTokenRequest = RequestCreator.CreateVerifyResetTokenRequest(resetToken);
        var verifyTokenResponse = await _client.PostAsJsonAsync(AuthApiPath.VerifyResetToken, verifyTokenRequest);

        // Assert - Verify token succeeds
        verifyTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var verifyTokenResult = await verifyTokenResponse.Content.ReadFromJsonAsync<VerifyResetToken.Response>();
        verifyTokenResult.Should().NotBeNull();
        verifyTokenResult.IsValid.Should().BeTrue();
    }

    private async Task<string> ResetRequest()
    {
        // Arrange
        var requestResetRequest = RequestCreator.CreateRequestResetPasswordRequest(_testUser.Email!);

        // Act - Request reset
        var requestResetResponse = await _client.PostAsJsonAsync(AuthApiPath.RequestResetPassword, requestResetRequest);

        // Assert - Request reset succeeds
        requestResetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var resetToken = SpyEmailSender.GetLastSentTokenToEmail(_testUser.Email!);
        resetToken.Should().NotBeNullOrEmpty();

        return resetToken;
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidToken = "invalid-token-123";

        // Act - Verify invalid token
        var verifyTokenRequest = RequestCreator.CreateVerifyResetTokenRequest(invalidToken);
        var verifyTokenResponse = await _client.PostAsJsonAsync(AuthApiPath.VerifyResetToken, verifyTokenRequest);

        // Assert
        verifyTokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithNonExistentEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentEmail = _faker.Internet.Email();
        var requestResetRequest = RequestCreator.CreateRequestResetPasswordRequest(nonExistentEmail);

        // Act
        var requestResetResponse = await _client.PostAsJsonAsync(AuthApiPath.RequestResetPassword, requestResetRequest);

        // Assert
        requestResetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidEmail = "invalid-email";
        var requestResetRequest = RequestCreator.CreateRequestResetPasswordRequest(invalidEmail);

        // Act
        var requestResetResponse = await _client.PostAsJsonAsync(AuthApiPath.RequestResetPassword, requestResetRequest);

        // Assert
        requestResetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private string GenerateValidPassword()
    {
        return _faker.Internet.Password();
    }
}