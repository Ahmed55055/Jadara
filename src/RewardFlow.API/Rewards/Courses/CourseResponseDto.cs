namespace RewardFlow_API.Rewards.Courses;

public record CourseResponseDto(
    int Id,
    string? Code,
    string Name,
    bool IsTheoretical,
    bool IsPractical,
    decimal SubjectPrice);