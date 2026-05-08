using Reward_Flow_v2.Common.Hashing;
using Reward_Flow_v2.Employees.Data;

namespace Reward_Flow_v2.Rewards.Data;

public class EmployeeSnapshot
{
    public Guid SnapshotId { get; private set; }
    public DateTime SnapshotDate { get; private set; }
    public int EmployeeId { get; set; }
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
    public float? Salary { get; set; }
    public byte? JobTitle { get; set; }
}
