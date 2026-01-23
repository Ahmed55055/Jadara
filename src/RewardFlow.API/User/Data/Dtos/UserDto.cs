namespace RewardFlow_API.User.Data.Dtos;

public record UserDto(Guid UUID, string Username, string? Email, DateTime CreatedAt, DateTime? LastVisit, bool IsActive);