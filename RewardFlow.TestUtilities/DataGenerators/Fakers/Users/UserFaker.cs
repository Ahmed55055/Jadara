using Bogus;
using Reward_Flow_v2.User.Data;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using System.Linq.Expressions;

namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Users;

/// <summary>
/// Generates fake user data for testing purposes, inheriting from <see cref="Faker{T}"/>.
/// </summary>
public class UserFaker : Faker<User>, IEntityFaker<User,UserFields>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserFaker"/> class.
    /// </summary>
    public UserFaker()
    {
        InitializeDefaults();
    }

    /// <summary>
    /// Sets up the default rules for generating user data.
    /// </summary>
    private void InitializeDefaults()
    {
        RuleFor(u => u.Username, f => f.Person.UserName);
        RuleFor(u => u.PasswordHash, f => f.Random.Hash().Substring(0, 20));
        RuleFor(u => u.Email, f => f.Person.Email.OrNull(f, .2f));
        RuleFor(u => u.RoleId, f => 3);
        RuleFor(u => u.CreatedAt, f => f.Date.Past(2));
        RuleFor(u => u.LastVisit, f => f.Date.Recent().OrNull(f, 0.15f));
        RuleFor(u => u.IsActive, f => true);
        RuleFor(u => u.ProfilePictureUrl, f => f.Internet.Avatar().OrNull(f, 0.5f));
        RuleFor(u => u.PlanId, f => f.Random.Int(1, 2));
        RuleFor(u => u.RefreshToken, f => f.Random.String2(64).OrNull(f, 0.7f));
        RuleFor(u => u.RefreshTokenExpiry, f => f.Date.Future(1).OrNull(f, 0.3f));
    }

    /// <summary>
    /// Overwrites existing rules to force specific fields to NULL for testing edge cases.
    /// </summary>
    /// <param name="fields">A flags enumeration that indicates which properties must be set to NULL.</param>
    public IEntityFaker<User,UserFields> WithNulls(UserFields fields)
    {
        if (fields.HasFlag(UserFields.Username)) RuleFor(u => u.Username, _ => null!);
        if (fields.HasFlag(UserFields.Email)) RuleFor(u => u.Email, _ => null);
        if (fields.HasFlag(UserFields.RoleId)) RuleFor(u => u.RoleId, _ => 0);
        if (fields.HasFlag(UserFields.PlanId)) RuleFor(u => u.PlanId, _ => 0);
        if (fields.HasFlag(UserFields.CreatedAt)) RuleFor(u => u.CreatedAt, _ => DateTime.MinValue);
        if (fields.HasFlag(UserFields.LastVisit)) RuleFor(u => u.LastVisit, _ => null);
        if (fields.HasFlag(UserFields.IsActive)) RuleFor(u => u.IsActive, _ => false);
        if (fields.HasFlag(UserFields.ProfilePictureUrl)) RuleFor(u => u.ProfilePictureUrl, _ => null);
        if (fields.HasFlag(UserFields.PasswordHash)) RuleFor(u => u.PasswordHash, _ => null!);
        if (fields.HasFlag(UserFields.RefreshToken)) RuleFor(u => u.RefreshToken, _ => null);
        if (fields.HasFlag(UserFields.RefreshTokenExpiry)) RuleFor(u => u.RefreshTokenExpiry, _ => null);

        return this;
    }

    /// <summary>
    /// Helper to force a property to a specific value without complex logic.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to set.</typeparam>
    /// <param name="property">The property expression.</param>
    /// <param name="value">The value to set for the property.</param>
    public IEntityFaker<User,UserFields> ForProperty<TProperty>(Expression<Func<User, TProperty>> property, TProperty value)
    {
        RuleFor(property, _ => value);
        return this;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UserFields"/> are populated with valid,
    /// non-null values, overriding any previous rule (including the default null-chance rules).
    /// </summary>
    /// <param name="fields">
    /// A flags enumeration that indicates which properties must receive a value.
    /// </param>
    public IEntityFaker<User,UserFields> WithValue(UserFields fields)
    {
        if (fields.HasFlag(UserFields.Username))
            RuleFor(u => u.Username, f => f.Person.UserName);

        if (fields.HasFlag(UserFields.Email))
            RuleFor(u => u.Email, f => f.Person.Email);

        if (fields.HasFlag(UserFields.RoleId))
            RuleFor(u => u.RoleId, f => f.Random.Int(1, 5));

        if (fields.HasFlag(UserFields.PlanId))
            RuleFor(u => u.PlanId, f => f.Random.Int(1, 3));

        if (fields.HasFlag(UserFields.CreatedAt))
            RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

        if (fields.HasFlag(UserFields.LastVisit))
            RuleFor(u => u.LastVisit, f => f.Date.Recent());

        if (fields.HasFlag(UserFields.IsActive))
            RuleFor(u => u.IsActive, f => true);

        if (fields.HasFlag(UserFields.ProfilePictureUrl))
            RuleFor(u => u.ProfilePictureUrl, f => f.Internet.Avatar());

        if (fields.HasFlag(UserFields.PasswordHash))
            RuleFor(u => u.PasswordHash, f => f.Random.Hash().Substring(0, 20));

        if (fields.HasFlag(UserFields.RefreshToken))
            RuleFor(u => u.RefreshToken, f => f.Random.String2(64));

        if (fields.HasFlag(UserFields.RefreshTokenExpiry))
            RuleFor(u => u.RefreshTokenExpiry, f => f.Date.Future(1));

        return this;
    }
}