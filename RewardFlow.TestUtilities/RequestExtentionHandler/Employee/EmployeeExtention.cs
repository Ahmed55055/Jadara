using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;

namespace RewardFlow.TestUtilities.RequestExtentionHandler.Employees;

public static class EmployeeExtention
{
    public static EmployeeRequest ToRequest(this Employee employee)
    {
        return new EmployeeRequest(employee);
    }
}