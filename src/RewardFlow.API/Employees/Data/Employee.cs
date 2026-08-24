using FluentResults;
using Reward_Flow_v2.Common.Hashing;
using System.Text.RegularExpressions;
using RewardFlow_API.Common.Interface;

namespace Reward_Flow_v2.Employees.Data;

public class Employee : ITenantEntity
{
    private List<EmployeeNameToken> _nameTokens = new();

    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;

    public string? NationalNumber
    {
        get;
        set
        {
            field = value;
            NationalNumberHash = HashField(value);
        }
    }

    public string? AccountNumber
    {
        get;
        set
        {
            field = value;
            AccountNumberHash = HashField(value);
        }
    }

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
    public IReadOnlyCollection<EmployeeNameToken> NameTokens => _nameTokens.AsReadOnly();

    public Employee() { }

    public static Employee? Create(string name, int createdBy, ICollection<EmployeeNameToken> nameTokens)
    {
        var employee = new Employee();

        var result = employee.UpdateName(name);

        if (result.IsFailed)
            return null;

        employee.CreatedBy = createdBy;
        employee.UpdateNameTokens(nameTokens);

        return employee;
    }

    /// <summary>
    /// Updates the name tokens, clearing the existing ones first
    /// </summary>
    /// <param name="nameTokens">The new name tokens</param>
    public void UpdateNameTokens(IEnumerable<EmployeeNameToken> nameTokens)
    {
        _nameTokens.Clear();
        _nameTokens.AddRange(nameTokens);
    }

    /// <summary>
    /// Updates the name after cleaning it up and doing basic validation
    /// </summary>
    /// <param name="rawName">The raw name input from the user</param>
    /// <returns>Result object indicating success or failure</returns>
    public Result UpdateName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return Result.Fail("Name cannot be empty.");

        // Clean up
        string cleanedName = Regex.Replace(rawName, @"[^a-zA-Z\u0600-\u06FF\s]", "");
        cleanedName = Regex.Replace(cleanedName, @"\s+", " ").Trim();

        // Final business validation check
        if (cleanedName.Length < 2)
            return Result.Fail("Name is too short after cleanup.");

        // State change ONLY happens if we get past all failures
        this.Name = cleanedName;

        return Result.Ok();
    }

    public void UpdateNationalNumber(string? nationalNumber)
    {
        if (string.IsNullOrWhiteSpace(nationalNumber))
        {
            NationalNumber = null;
            return;
        }

        string cleanedNumber = Regex.Replace(nationalNumber, @"[^0-9]", "");

        NationalNumber = string.IsNullOrEmpty(cleanedNumber) ? null : cleanedNumber;
    }
    
    public static string? HashField(string? value)
    {
        return value != null ? XxHasher.Hash(value) : null;
    }
}