using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.CreateEmployee;
using Reward_Flow_v2.Employees.DeleteEmployee;
using Reward_Flow_v2.Employees.GetAllEmployees;
using Reward_Flow_v2.Employees.GetEmployeeById;
using Reward_Flow_v2.Employees.GetEmployeeByName;
using Reward_Flow_v2.Employees.GetEmployeeByNationalNumber;
using Reward_Flow_v2.Employees.SearchEmployeesByName;
using Reward_Flow_v2.Employees.UpdateEmployee;

namespace Reward_Flow_v2.Employees;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateEmployee();
        app.MapGetAllEmployees();
        app.MapGetEmployeeById();
        app.MapGetEmployeeByName();
        app.MapGetEmployeeByNationalNumber();
        app.MapSearchEmployeesByName();
        app.MapUpdateEmployee();
        app.MapDeleteEmployee();
        app.MapBulkInsertEmployee();
        app.MapBulkInsertEmployeeV2();
    }
}