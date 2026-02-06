namespace Reward_Flow_v2.Employees;

public static class EmployeeApiPath
{
    public const string Tag = "Employees";
    private const string EmployeeRootApi = $"{ApiPath.Route}/{Tag}";

    public const string Create = $"{EmployeeRootApi}";
    public const string GetAll = $"{EmployeeRootApi}";
    public const string GetById = $"{EmployeeRootApi}/{{id}}";
    public const string GetByName = $"{EmployeeRootApi}/name/{{name}}";
    public const string GetByNationalNumber = $"{EmployeeRootApi}/national/{{nationalNumber}}";
    public const string SearchByName = $"{EmployeeRootApi}/search";
    public const string Update = $"{EmployeeRootApi}/{{id}}";
    public const string Delete = $"{EmployeeRootApi}/{{id}}";
    public const string BulkInsert = $"{EmployeeRootApi}/BulkInsert";
}