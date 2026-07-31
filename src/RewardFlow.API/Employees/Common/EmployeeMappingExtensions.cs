using Reward_Flow_v2.Employees.Data;

namespace RewardFlow_API.Employees.Common;

public static class EmployeeMappingExtensions
{
    public static EmployeeDto ToDto(this Employee e) => new()
    {
        Id = e.EmployeeId,
        Name = e.Name,
        NationalNumber = e.NationalNumber,
        AccountNumber = e.AccountNumber,
        Salary = e.Salary,
        Faculty = e.Faculty?.Name,
        Department = e.Department?.Name,
        CreatedAt = e.CreatedAt,
        JobTitle = e.JobTitle,
        IsActive = e.IsActive,
        Status = e.Status
    };
}
