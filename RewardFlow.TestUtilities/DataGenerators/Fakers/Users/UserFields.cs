namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Users;

[Flags]
public enum UserFields
{
    None = 0,
    Username = 1,
    Email = 2,
    RoleId = 4,
    PlanId = 8,
    CreatedAt = 16,
    LastVisit = 32,
    IsActive = 64,
    ProfilePictureUrl = 128,
    PasswordHash = 256,
    RefreshToken = 512,
    RefreshTokenExpiry = 1024
}