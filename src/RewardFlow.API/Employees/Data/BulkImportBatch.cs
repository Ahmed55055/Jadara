using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Employees.Data;

public class BulkImportBatch: ITenantEntity
{
    public Guid Id {get; init;}
    public DateTime Date {get; init;}
    public int UserId {get; init;}
    public string Status {get; private set;} = "Pending";
    public int TotalRecords { get; set; }
    public int SuccessCount { get; private set; } = 0;
    public string RawPayloadJson {get; init;}
    
    public bool IsClosed => Status is "Completed" or "Canceled";
    public Guid TenantId { get; set; }
    public void Proccessing() => Status = "Processing";
    public void Completed() => Status = "Completed";
    public void Completed(int successCount) {
        SuccessCount = successCount;
        Status = "Completed";
    }
    public void Canceled() => Status = "Canceled";
    public void Failed() => Status = "Failed";
}