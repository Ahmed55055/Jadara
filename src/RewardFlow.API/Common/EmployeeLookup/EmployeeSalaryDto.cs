namespace Reward_Flow_v2.Common.EmployeeLookup;

public record EmployeeSalaryDto
{
    public int EmployeeId { get; init; }
    public decimal Salary { get; init; }
}