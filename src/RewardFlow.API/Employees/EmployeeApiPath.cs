namespace Reward_Flow_v2.Employees;

public static class EmployeeApiPath
{
    public const string Tag = "Employees";
    private const string EmployeeRootApi = $"{ApiPath.Route}/{Tag}";

    // Single Operations
    public const string Create = $"{EmployeeRootApi}";
    public const string GetAll = $"{EmployeeRootApi}";
    public const string GetById = $"{EmployeeRootApi}/{{id}}";
    public const string GetByName = $"{EmployeeRootApi}/name/{{name}}";
    public const string GetByNationalNumber = $"{EmployeeRootApi}/national/{{nationalNumber}}";
    public const string SearchByName = $"{EmployeeRootApi}/search";
    public const string Update = $"{EmployeeRootApi}/{{id}}";
    public const string Delete = $"{EmployeeRootApi}/{{id}}";
    
    // Bulk
    public const string BulkInsert = $"{EmployeeRootApi}/BulkInsert";
    public const string BulkInsertResult = $"{EmployeeRootApi}/BulkInsert/{{batchId}}";
    public const string BulkInsertV2 = $"{EmployeeRootApi}/v2/BulkInsert";
    
    public const string ConflictCheck = $"{EmployeeRootApi}/conflict-check";
    public const string GetBatch = $"{EmployeeRootApi}/query";
}