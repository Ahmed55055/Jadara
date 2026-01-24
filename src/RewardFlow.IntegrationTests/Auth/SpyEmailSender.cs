using RewardFlow.IntegrationTests.Auth.Common;

namespace RewardFlow.IntegrationTests.Auth;

public class SpyEmailSender: IResetPasswordMessageSender
{
    static readonly Dictionary<string, List<string>> SentTokens = new();
    
    public async Task SendToken(string email,string token)
    {
        if(!SentTokens.ContainsKey(email))
            SentTokens.Add(email,new());
            
        SentTokens[email].Add(token);
    }
    
    public static string? GetLastSentTokenToEmail(string email)
    {
        SentTokens.TryGetValue(email, out var tokenList);
        return tokenList?.LastOrDefault();
    }
}