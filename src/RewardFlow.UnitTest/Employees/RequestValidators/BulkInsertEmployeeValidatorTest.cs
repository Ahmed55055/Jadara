using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;
using RewardFlow_UnitTest.Infurstructure;
using RewardFlow.TestUtilities.RequestExtentionHandler.Employees;

namespace RewardFlow_UnitTest.Employees.RequestValidators;

public class BulkInsertEmployeeValidatorTest: 
    RequestValidatorTestBase<Employee,BulkInsertEmployeeRequestValidator,BulkRequest, IEnumerable<Employee>>
{
    protected override BulkRequest MapToRequest(IEnumerable<Employee> entity) =>
        entity.ToRequest().BulkInsert();

    // DEV NOTE:
    // This is a secondary feature in the system,
    // so we will implement it later after testing the core parts that will may have huge legal concerns
    public override Task InvalidRequest_ShouldReturnInvalid(IEnumerable<Employee> input, TestCaseInfo testCaseInfo) 
    {
        throw new NotImplementedException();
    }

    public override Task ValidRequest_ShouldReturnValid(IEnumerable<Employee> input, TestCaseInfo testCaseInfo)
    {
        throw new NotImplementedException();
    }
}