using Bogus;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Auth.Common;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.AuthService.Register;
using Reward_Flow_v2.User.AuthService.Login;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Users;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the User Full Lifecycle.
/// Tests the complete user workflow from registration through login to password reset.
/// </summary>
[Collection("UserTests")]
public class UserFullLifecycleTests : IAsyncLifetime
{
    private readonly Faker _faker = new();
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client;
    private Reward_Flow_v2.User.Data.User _testUser;
    private string _originalPassword;

    public UserFullLifecycleTests(UserTestFixture factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UserLifecycle_FullWorkflow_ShouldWorkCorrectly()
    {
        // Test 1: Register User
        await RegisterUser();

        // Test 2: Login with Original Password
        await LoginWithOriginalPassword();

        // Test 3: Request Password Reset
        string resetToken = await RequestPasswordReset();

        // Test 4: Verify Reset Token
        await VerifyResetToken(resetToken);

        // Test 5: Confirm Password Reset
        string newPassword = await ConfirmPasswordReset(resetToken);

        // Test 6: Login with New Password
        await LoginWithNewPassword(newPassword);

        // Test 7: Verify Old Password No Longer Works
        await VerifyOldPasswordFails();
    }

    private async Task RegisterUser()
    {
        _testUser = TestDataGenerator.User.WithValue(UserFields.Email).Generate();
        _originalPassword = _faker.Internet.Password();
        var request = RequestCreator.CreateRegisterRequest(_testUser, _originalPassword);

        var response = await _client.PostAsJsonAsync(AuthApiPath.Register, request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Register.Response>();
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.JwtToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be(_testUser.Username);
        result.User.Email.Should().Be(_testUser.Email);
    }

    private async Task LoginWithOriginalPassword()
    {
        var request = RequestCreator.CreateLoginRequest(_testUser, _originalPassword);

        var response = await _client.PostAsJsonAsync(AuthApiPath.Login, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Login.Response>();
        result.Should().NotBeNull();
        result.JWTToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    private async Task<string> RequestPasswordReset()
    {
        var request = RequestCreator.CreateRequestResetPasswordRequest(_testUser.Email!);

        var response = await _client.PostAsJsonAsync(AuthApiPath.RequestResetPassword, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetToken = SpyEmailSender.GetLastSentTokenToEmail(_testUser.Email!);
        resetToken.Should().NotBeNullOrEmpty();

        return resetToken;
    }

    private async Task VerifyResetToken(string resetToken)
    {
        var request = RequestCreator.CreateVerifyResetTokenRequest(resetToken);

        var response = await _client.PostAsJsonAsync(AuthApiPath.VerifyResetToken, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<VerifyResetToken.Response>();
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    private async Task<string> ConfirmPasswordReset(string resetToken)
    {
        var newPassword = _faker.Internet.Password();
        var request = RequestCreator.CreateConfirmResetPasswordRequest(resetToken, newPassword);

        var response = await _client.PostAsJsonAsync(AuthApiPath.ConfirmResetPassword, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConfirmResetPassword.Response>();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        return newPassword;
    }

    private async Task LoginWithNewPassword(string newPassword)
    {
        var request = RequestCreator.CreateLoginRequest(_testUser, newPassword);

        var response = await _client.PostAsJsonAsync(AuthApiPath.Login, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Login.Response>();
        result.Should().NotBeNull();
        result.JWTToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    private async Task VerifyOldPasswordFails()
    {
        var request = RequestCreator.CreateLoginRequest(_testUser, _originalPassword);

        var response = await _client.PostAsJsonAsync(AuthApiPath.Login, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}