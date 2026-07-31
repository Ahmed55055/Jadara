using Reward_Flow_v2.Employees.BulkInsertEmployees;

namespace Reward_Flow_v2.Employees.Data;
public class BulkImportResult
{
    public int Id { get; private set; }
    public Guid BatchId { get; private set; }
    public Guid Tracker { get; private set; }
    public bool IsSuccess { get; private set; }
    public int? EmployeeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ErrorTypeCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Empty constructor for EF Core
    private BulkImportResult() { }

    // Named constructors to make creation expressive and eliminate long inline object mapping blocks
    public static BulkImportResult CreateSuccess(Guid batchId, Guid tracker, int employeeId, string name)
    {
        return new BulkImportResult
        {
            BatchId = batchId,
            Tracker = tracker,
            IsSuccess = true,
            EmployeeId = employeeId,
            Name = name
        };
    }

    public static BulkImportResult CreateFailure(Guid batchId, Guid tracker, BulkInsert.ErrorTypes errorType, string message)
    {
        return new BulkImportResult
        {
            BatchId = batchId,
            Tracker = tracker,
            IsSuccess = false,
            ErrorTypeCode = errorType.ToString(),
            ErrorMessage = message
        };
    }
}