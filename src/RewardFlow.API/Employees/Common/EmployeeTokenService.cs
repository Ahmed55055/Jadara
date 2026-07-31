using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.Tokenization;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;

namespace RewardFlow_API.Employees.Common;

public class EmployeeTokenService : IEmployeeTokenService
{
    private readonly EmployeeDbContext _dbContext;
    private readonly ITokenizer _tokenizer;

    public EmployeeTokenService(EmployeeDbContext dbContext, ITokenizer tokenizer)
    {
        _dbContext = dbContext;
        _tokenizer = tokenizer;
    }

    public IEnumerable<EmployeeNameToken> CreateTokens(IEnumerable<Employee> employees, int userId,
        CancellationToken cancellationToken = default)
    {
        var tokens = new List<EmployeeNameToken>();

        foreach (var employee in employees)
        {
            tokens.AddRange(GenerateTokens(employee.Name, employee.EmployeeId, userId));
        }

        return tokens;
    }

    public IEnumerable<EmployeeNameToken> CreateTokens(Employee employee, int userId,
        CancellationToken cancellationToken = default)
    {
        var tokens = new List<EmployeeNameToken>();

        tokens.AddRange(GenerateTokens(employee.Name, employee.EmployeeId, userId));

        return tokens;
    }

    public async Task CreateTokensAsync(Employee employee, int userId, CancellationToken cancellationToken = default)
    {
        var tokens = GenerateTokens(employee.Name, employee.EmployeeId, userId);
        _dbContext.EmployeeNameToken.AddRange(tokens);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTokensAsync(Employee employee, int userId, CancellationToken cancellationToken = default)
    {
        await DeleteTokensAsync(employee.EmployeeId, userId, cancellationToken);
        await CreateTokensAsync(employee, userId, cancellationToken);
    }

    public async Task DeleteTokensAsync(int employeeId, int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.EmployeeNameToken
            .Where(t => t.EmployeeId == employeeId && t.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.EmployeeNameToken.RemoveRange(tokens);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<int>> SearchEmployeesByNameAsync(string searchName, int userId, int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var searchTokens = GenerateSearchTokens(searchName);

        var results = await _dbContext.EmployeeNameToken
            .Where(t => t.UserId == userId && searchTokens.Contains(t.TokenHashed))
            .GroupBy(t => t.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .Select(x => x.EmployeeId)
            .ToListAsync(cancellationToken);

        return results;
    }

    public List<EmployeeNameToken> CreateTokens(string employeeName, int employeeId, int userId)
    {
        return GenerateTokens(employeeName, employeeId, userId).ToList();
    }

    private IEnumerable<EmployeeNameToken> GenerateTokens(string name, int employeeId, int userId)
    {
        var tokens = new List<EmployeeNameToken>();

        // 2-gram tokens
        var twoGrams = _tokenizer.TokenizeToNGrams(name, 2, false);
        tokens.AddRange(twoGrams.Select(token => new EmployeeNameToken
        {
            UserId = userId, TokenHashed = _tokenizer.HashToken(token), N = 2, EmployeeId = employeeId
        }));

        // 3-gram tokens with spaces
        var threeGrams = _tokenizer.TokenizeToNGrams(name, 3, true);
        tokens.AddRange(threeGrams.Select(token => new EmployeeNameToken
        {
            UserId = userId, TokenHashed = _tokenizer.HashToken(token), N = 3, EmployeeId = employeeId
        }));

        return tokens;
    }

    private IEnumerable<string> GenerateSearchTokens(string searchName)
    {
        var tokens = new List<string>();

        var twoGrams = _tokenizer.TokenizeToNGrams(searchName, 2, false);
        tokens.AddRange(twoGrams.Select(_tokenizer.HashToken));

        var threeGrams = _tokenizer.TokenizeToNGrams(searchName, 3, true);
        tokens.AddRange(threeGrams.Select(_tokenizer.HashToken));

        return tokens;
    }
}