using Reward_Flow_v2.Employees.Data;
using System.Linq.Expressions;

namespace RewardFlow.IntegrationTests.Infrastructure.DataGenerators;

using Bogus;

public class EmployeeFaker : Faker<Employee>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeFaker"/> class.
    /// </summary>
    public EmployeeFaker()
    {
        this.Locale = "ar";
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        RuleFor(e => e.Name, f => f.Name.FullName());

        RuleFor(e => e.NationalNumber, f => f.Random.String2(14, "0123456789").OrNull(f, 0.1f));
        RuleFor(e => e.AccountNumber, f => f.Random.String2(f.Random.Number(12, 14), "0123456789-").OrNull(f, 0.1f));

        RuleFor(e => e.Salary, f => f.Random.Float(250, 15000).OrNull(f, 0.1f));
        RuleFor(e => e.FacultyId, f => f.Random.Int(1, 2).OrNull(f, 0.2f));
        RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 3).OrNull(f, 0.2f));
        RuleFor(e => e.JobTitle, f => f.Random.Byte(1, 3).OrNull(f, 0.1f));
        RuleFor(e => e.Status, f => f.Random.Byte(1, 3).OrNull(f, 0.1f));

        RuleFor(e => e.IsActive, f => f.Random.Bool(0.85f));
        RuleFor(e => e.CreatedBy, f => f.Random.Int(1, 100));
        RuleFor(e => e.CreatedAt, f => f.Date.Past(2));
    }

    /// <summary>
    /// Overwrites existing rules to force specific fields to NULL for testing edge cases.
    /// </summary>
    public EmployeeFaker WithNulls(EmployeeFields fields)
    {
        if (fields.HasFlag(EmployeeFields.NationalNumber)) RuleFor(e => e.NationalNumber, _ => null);
        if (fields.HasFlag(EmployeeFields.AccountNumber)) RuleFor(e => e.AccountNumber, _ => null);
        if (fields.HasFlag(EmployeeFields.Salary)) RuleFor(e => e.Salary, _ => null);
        if (fields.HasFlag(EmployeeFields.FacultyId)) RuleFor(e => e.FacultyId, _ => null);
        if (fields.HasFlag(EmployeeFields.DepartmentId)) RuleFor(e => e.DepartmentId, _ => null);
        if (fields.HasFlag(EmployeeFields.JobTitle)) RuleFor(e => e.JobTitle, _ => null);
        if (fields.HasFlag(EmployeeFields.Status)) RuleFor(e => e.Status, _ => null);

        return this;
    }

    /// <summary>
    /// Helper to force a property to a specific value without complex logic.
    /// </summary>
    public EmployeeFaker ForProperty<TProperty>(Expression<Func<Employee, TProperty>> property, TProperty value)
    {
        RuleFor(property, _ => value);
        return this;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EmployeeFields"/> are populated with valid,
    /// non-null values, overriding any previous rule (including the default null-chance rules).
    /// </summary>
    /// <param name="fields">
    /// A flags enumeration that indicates which properties must receive a value.
    /// </param>
    public EmployeeFaker WithValue(EmployeeFields fields)
    {
        if (fields.HasFlag(EmployeeFields.NationalNumber))
            RuleFor(e => e.NationalNumber, f => f.Random.String2(14, "0123456789"));

        if (fields.HasFlag(EmployeeFields.AccountNumber))
            RuleFor(e => e.AccountNumber, f => f.Random.String2(f.Random.Number(12, 14), "0123456789-"));

        if (fields.HasFlag(EmployeeFields.Salary))
            RuleFor(e => e.Salary, f => f.Random.Float(250, 15_000));

        if (fields.HasFlag(EmployeeFields.FacultyId))
            RuleFor(e => e.FacultyId, f => f.Random.Int(1, 2));

        if (fields.HasFlag(EmployeeFields.DepartmentId))
            RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 3));

        if (fields.HasFlag(EmployeeFields.JobTitle))
            RuleFor(e => e.JobTitle, f => f.Random.Byte(1, 3));

        if (fields.HasFlag(EmployeeFields.Status))
            RuleFor(e => e.Status, f => f.Random.Byte(1, 3));

        return this;
    }
}