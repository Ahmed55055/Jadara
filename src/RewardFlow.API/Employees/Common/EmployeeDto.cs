namespace Reward_Flow_v2.Employees.Common;

public record EmployeeDto{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? NationalNumber { get; set; }
    public string? AccountNumber { get; set; }
    public float? Salary { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte? JobTitle { get; set; }
    public bool IsActive { get; set; }
    public byte? Status { get; set; }
}