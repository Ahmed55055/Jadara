using Reward_Flow_v2.User.Data;
using RewardFlow_API.User.AuthService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class UserClient
{
    private readonly TestWebApplicationFactory _factory;
    
    /// <summary>
    /// Gets the HTTP client configured for making authenticated requests.
    /// </summary>
    public HttpClient Client { get; private set; }
    
    /// <summary>
    /// Gets the user associated with this client.
    /// </summary>
    public User User { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserClient"/> class.
    /// </summary>
    /// <param name="factory">The test web application factory used to create the HTTP client.</param>
    /// <param name="user">The user to authenticate and associate with the client.</param>
    public UserClient(TestWebApplicationFactory factory, User user)
    {
        _factory = factory;
        Client = _factory.CreateClient();
        User = user;
        Authanticate();
    }
    
    public void Authanticate()
    {
        var jwtToken = TokenService.CreateToken(User, _factory.Configuration);
        Client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", jwtToken);
    }
}