using Reward_Flow_v2.Common.Hashing;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Employees.Data;

public class Employee : ITenantEntity
{
    public int EmployeeId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? NationalNumber 
    { 
        get => _nationalNumber; 
        set 
        {
            _nationalNumber = value;
            NationalNumberHash = value != null ? XxHasher.Hash(value) : null;
        }
    }
    private string? _nationalNumber;
    
    public string? AccountNumber 
    { 
        get => _accountNumber; 
        set 
        {
            _accountNumber = value;
            AccountNumberHash = value != null ? XxHasher.Hash(value) : null;
        }
    }
    private string? _accountNumber;
    
    public string? NationalNumberHash { get; private set; }
    public string? AccountNumberHash { get; private set; }
    public decimal? Salary { get; set; }
    public int? FacultyId { get; set; }
    public int? DepartmentId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte? JobTitle { get; set; }
    public bool IsActive { get; set; }
    public byte? Status { get; set; }

    public virtual Department? Department { get; set; }
    public virtual Faculty? Faculty { get; set; }
    public virtual JobTitle? JobTitleNavigation { get; set; }
    public virtual EmployeeStatus? StatusNavigation { get; set; }

}
