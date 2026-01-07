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
    
    public HttpClient Client { get; private set; }
    public User User { get; private set; }

    public UserClient(TestWebApplicationFactory factory, User user)
    {
        _factory = factory;
        Client = _factory.CreateClient();
        User = user;
    }
    
    public void Authanticate()
    {
        var jwtToken = TokenService.CreateToken(User, _factory.Configuration);
        Client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", jwtToken);
    }
}