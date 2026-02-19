using Reward_Flow_v2.Employees.Data;
using RewardFlow_UnitTest.Employees.PropertyCases;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.Extentions;
using System.Linq.Expressions;

namespace RewardFlow_UnitTest.Employees;

public class EmployeeValidationMemberData
{
    private static readonly IEnumerable<object[]> ValidPropertyCases = GenerateValidTestCases();
    private static readonly IEnumerable<object[]> InvalidPropertyCases = GenerateInvalidTestCases();

    private static IEnumerable<object[]> GenerateValidTestCases() =>
        TestDataCaseGenerator.Create(() => TestDataGenerator.Employee)
            .AddCases(EmployeePropertyCases.Valid.Names, e => e.Name)
            .AddCases(EmployeePropertyCases.Valid.NationalNums, e => e.NationalNumber)
            .AddCases(EmployeePropertyCases.Valid.AccountNums, e => e.AccountNumber)
            .AddCases(EmployeePropertyCases.Valid.Salaries, e => e.Salary)
            .AddCases(EmployeePropertyCases.Valid.ForeignKeysId, e => e.DepartmentId)
            .AddCases(EmployeePropertyCases.Valid.ForeignKeysId, e => e.FacultyId)
            .GenerateCases()
            .ToList(); // Don't Remove, this is necessary to materialize the list immediately. (to not regenerate the data on each loop)

    private static IEnumerable<object[]> GenerateInvalidTestCases() =>
        TestDataCaseGenerator.Create(() => TestDataGenerator.Employee)
            .AddCases(EmployeePropertyCases.Invalid.Name, e => e.Name)
            .AddCases(EmployeePropertyCases.Invalid.NationalNums, e => e.NationalNumber)
            .AddCases(EmployeePropertyCases.Invalid.AccountNums, e => e.AccountNumber)
            .AddCases(EmployeePropertyCases.Invalid.Salary, e => e.Salary)
            .AddCases(EmployeePropertyCases.Invalid.ForeignKeysId, e => e.DepartmentId)
            .AddCases(EmployeePropertyCases.Invalid.ForeignKeysId, e => e.FacultyId)
            .GenerateCases()
            .ToList(); // Don't Remove, this is necessary to materialize the list immediately. (to not regenerate the data on each loop)

    public static IEnumerable<object[]> ValidTestDataBoundary() => ValidPropertyCases;
    public static IEnumerable<object[]> InvalidBoundary() => InvalidPropertyCases;
    public static IEnumerable<object[]> SingleInvalidProperty<TProperty>(
        Expression<Func<Employee, TProperty>> property) =>
        InvalidPropertyCases
            .Where(t => t[1] is TestCaseInfo testCaseInfo && testCaseInfo.PropertyName == property.GetPropertyName())
            .ToList();
}