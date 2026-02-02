namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;

[Flags]
public enum EmployeeFields
{
    None = 0,
    NationalNumber = 1,
    AccountNumber = 2,
    Salary = 4,
    FacultyId = 8,
    DepartmentId = 16,
    JobTitle = 32,
    Status = 64,
}