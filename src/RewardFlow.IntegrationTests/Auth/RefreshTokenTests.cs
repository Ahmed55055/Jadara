using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.AuthService;
using Reward_Flow_v2.User.AuthService.Login;
using Reward_Flow_v2.User.AuthService.Register;
using RewardFlow.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Auth;

public class RefreshTokenTests(TestWebApplicationFactory factory) : BaseAuthTestFixture(factory), IAsyncLifetime
{
    private HttpClient client = default!;
    FakeTimeProvider fakeTime;
    private Register.Response register;

    public new async Task InitializeAsync()
    {
        fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var app = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(fakeTime);
            });
        });

        client = app.CreateClient();
        register = await CreateUser();
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        await base.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RefreshToken_AfterJwtExpiration_ShouldReturnOk()
    {
        // Arrange
        fakeTime.Advance(TimeSpan.FromMinutes(11));

        // Act
        var response = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken, new RefreshToken.Request(register.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_BeforeJwtExpiration_ShouldReturnOk()
    {
        // Arrange
        fakeTime.Advance(TimeSpan.FromMinutes(5));

        // Act
        var response = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken, new RefreshToken.Request(register.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_ShouldIssueNewJwt()
    {
        // Act
        var response = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken, new RefreshToken.Request(register.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RefreshToken.Response>();

        result!.JWTToken.Should().NotBe(register.JwtToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldRotateRefreshToken()
    {
        // Act
        var response = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken, new RefreshToken.Request(register.RefreshToken));
        
        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RefreshToken.Response>();

        result!.RefreshToken.Should().NotBe(register.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_OldRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var first = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken,
            new RefreshToken.Request(register.RefreshToken));

        first.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var second = await client.PostAsJsonAsync(
            AuthApiPath.RefreshToken,
            new RefreshToken.Request(register.RefreshToken));

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    private async Task<Register.Response> CreateUser()
    {
        const string failMessage = "Failed at created user arrangement";

        var request = new Register.Request(_faker.Internet.UserName(), _faker.Internet.Password(),
            _faker.Internet.Email());
        var httpResponseMessage = await client.PostAsJsonAsync(AuthApiPath.Register, request);
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created, failMessage);

        var result = await httpResponseMessage.Content.ReadFromJsonAsync<Register.Response>();
        result.Should().NotBeNull(failMessage);

        return result;
    }
}