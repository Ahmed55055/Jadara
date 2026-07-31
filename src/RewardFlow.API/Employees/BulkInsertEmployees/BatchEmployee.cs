namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

public record BatchEmployee(
    Guid Tracker,
    string Name,
    string? NationalNumber = null,
    string? AccountNumber = null,
    decimal? Salary = null);