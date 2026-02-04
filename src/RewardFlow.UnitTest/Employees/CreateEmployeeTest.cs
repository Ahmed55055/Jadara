using Reward_Flow_v2.Employees.Data;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using System.Linq.Expressions;
using System.Text;

namespace RewardFlow_UnitTest.Employees;

public class CreateEmployeeTest
{
    private readonly EmployeeTestFixture _factory;

    public CreateEmployeeTest(EmployeeTestFixture factory)
    {
        _factory = factory;
    }

    [Theory]
    [MemberData(nameof(InvalidEmployees))]
    public async Task CreateEmployee_WithInvalidField_ShouldReturnBadRequest(Employee employee, string reason)
    {
    }

    public static IEnumerable<object[]> InvalidEmployees()
    {
        yield return [TestDataGenerator.Employee.WithNulls(EmployeeFields.Name).Generate(), "Empty name"];

        IEnumerable<object[]> AllInvalidGeneratedEmployeesCases = new[]
        {
            GenerateInvalidEmployees(InvalidEmployeeDataCases.NationalNums, e => e.NationalNumber),
            GenerateInvalidEmployees(InvalidEmployeeDataCases.AccountNums, e => e.AccountNumber),
            GenerateInvalidEmployees(InvalidEmployeeDataCases.Salary, e => e.Salary),
            GenerateInvalidEmployees(InvalidEmployeeDataCases.ForeignKeysId, e => e.FacultyId),
            GenerateInvalidEmployees(InvalidEmployeeDataCases.ForeignKeysId, e => e.DepartmentId),
            GenerateInvalidEmployees(InvalidEmployeeDataCases.ForeignKeysId, e => e.Status)
        }.SelectMany(x => x);

        foreach (var employeesCase in AllInvalidGeneratedEmployeesCases)
            yield return employeesCase;
    }

    public static IEnumerable<object[]> GenerateInvalidEmployees<TProprity>(
        IEnumerable<(TProprity value, string reason)> cases, Expression<Func<Employee, TProprity>> employee)
    {
        foreach (var (value, reason) in cases)
        {
            yield return [TestDataGenerator.Employee.ForProperty(employee, value).Generate(), reason];
        }
    }
}