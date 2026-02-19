using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.UpdateEmployee;
using RewardFlow_UnitTest.Infurstructure;
using RewardFlow.TestUtilities.RequestExtentionHandler.Employees;

namespace RewardFlow_UnitTest.Employees.RequestValidators;

public class UpdateEmployeeValidatorTest : 
    RequestValidatorTestBase<Employee , UpdateEmployeeRequestValidator ,UpdateEmployee.Request,Employee>
{
    protected override UpdateEmployee.Request MapToRequest(Employee entity) => entity.ToRequest().Update();
    
    [Theory]
    [MemberData(
        nameof(EmployeeValidationMemberData.InvalidBoundary), 
        MemberType = typeof(EmployeeValidationMemberData))]
    public override async Task InvalidRequest_ShouldReturnInvalid(Employee employee, TestCaseInfo testCaseInfo) =>
        await base.RunTest(employee, testCaseInfo, shouldBeValid: false);
    
    [Theory]
    [MemberData(
        nameof(EmployeeValidationMemberData.ValidTestDataBoundary), 
        MemberType = typeof(EmployeeValidationMemberData))]
    public override async Task ValidRequest_ShouldReturnValid(Employee employee, TestCaseInfo testCaseInfo)=>
        await base.RunTest(employee, testCaseInfo, shouldBeValid: true);
}