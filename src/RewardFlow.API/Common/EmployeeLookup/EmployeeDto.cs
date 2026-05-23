namespace Reward_Flow_v2.Common.EmployeeLookup;

public record EmployeeDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = null!;
    public decimal? Salary { get; init; }
    public string? NationalNumber { get; init; }
}
