using Bogus;
using Reward_Flow_v2.User.Data;

namespace RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

public class UserGenerator
{
    private int _seed = 0;
    private string? _fixedUsername;
    private string? _fixedEmail;
    private bool? _fixedIsActive;
    private int? _fixedRoleId;
    private int? _fixedPlanId;
    private DateTime? _fixedCreatedAt;
    private DateTime? _fixedLastVisit;
    private string? _fixedProfilePictureUrl;
    private string? _fixedPasswordHash;
    private string? _fixedRefreshToken;
    private DateTime? _fixedRefreshTokenExpiry;

    public UserGenerator Seed(int seed)
    {
        _seed = seed;
        return this;
    }

    public UserGenerator Username(string username)
    {
        _fixedUsername = username;
        return this;
    }

    public UserGenerator Email(string? email)
    {
        _fixedEmail = email;
        return this;
    }

    public UserGenerator IsActive(bool isActive)
    {
        _fixedIsActive = isActive;
        return this;
    }

    public UserGenerator RoleId(int roleId)
    {
        _fixedRoleId = roleId;
        return this;
    }

    public UserGenerator PlanId(int planId)
    {
        _fixedPlanId = planId;
        return this;
    }

    public UserGenerator CreatedAt(DateTime createdAt)
    {
        _fixedCreatedAt = createdAt;
        return this;
    }

    public UserGenerator LastVisit(DateTime? lastVisit)
    {
        _fixedLastVisit = lastVisit;
        return this;
    }

    public UserGenerator ProfilePictureUrl(string? profilePictureUrl)
    {
        _fixedProfilePictureUrl = profilePictureUrl;
        return this;
    }

    public UserGenerator PasswordHash(string passwordHash)
    {
        _fixedPasswordHash = passwordHash;
        return this;
    }

    public UserGenerator RefreshToken(string? refreshToken)
    {
        _fixedRefreshToken = refreshToken;
        return this;
    }

    public UserGenerator RefreshTokenExpiry(DateTime? refreshTokenExpiry)
    {
        _fixedRefreshTokenExpiry = refreshTokenExpiry;
        return this;
    }

    public User Generate()
    {
        var faker = new Faker<User>();
        if (_seed != 0) faker.UseSeed(_seed);

        faker
            .RuleFor(u => u.Username, f => _fixedUsername ?? f.Person.UserName)
            .RuleFor(u => u.PasswordHash, f => _fixedPasswordHash ?? f.Random.Hash().Substring(0, 20))
            .RuleFor(u => u.Email, f => _fixedEmail ?? f.Person.Email.OrNull(f, .2f))
            .RuleFor(u => u.RoleId, f => _fixedRoleId ?? 3)
            .RuleFor(u => u.CreatedAt, f => _fixedCreatedAt ?? f.Date.Past(2))
            .RuleFor(u => u.LastVisit, f => _fixedLastVisit ?? f.Date.Recent().OrNull(f, 0.15f))
            .RuleFor(u => u.IsActive, _fixedIsActive ?? true)
            .RuleFor(u => u.ProfilePictureUrl, f => _fixedProfilePictureUrl ?? f.Internet.Avatar().OrNull(f, 0.5f))
            .RuleFor(u => u.PlanId, f => _fixedPlanId ?? f.Random.Int(1, 2))
            .RuleFor(u => u.RefreshToken, f => _fixedRefreshToken ?? f.Random.String2(64).OrNull(f, 0.7f))
            .RuleFor(u => u.RefreshTokenExpiry, f => _fixedRefreshTokenExpiry ?? f.Date.Future(1).OrNull(f, 0.3f));

        return faker.Generate();
    }

    public List<User> Generate(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Generate()).ToList();
    }
}