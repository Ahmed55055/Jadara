using Bogus;
using Reward_Flow_v2.Employees.Data;
using System.Linq.Expressions;
using System.Text;

namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;

public class EmployeeFaker : Faker<Employee>, IEntityFaker<Employee, EmployeeFields>
{
    private static readonly string[] ArabicMaleNames =
    [
        "محمد", "أحمد", "علي", "عمر", "عبد الله", "يوسف", "إبراهيم", "حسن", "حسين", "خالد", "وليد", "طارق", "زيد",
        "حمزة", "بلال", "عثمان", "سعيد", "فيصل", "ناصر", "ماجد", "راشد", "سمير", "كريم", "جميل", "عادل", "فارس",
        "مالك", "ريان", "أمين", "سالم", "ياسين", "إدريس", "هارون", "زكريا", "إسماعيل", "مصطفى", "عمرو", "آسر", "أيمن",
        "حازم", "شريف", "باسم", "سيف", "تامر", "مهند", "مروان", "أمجد", "سلطان", "راضي", "نادر", "فادي", "غالي", "جواد",
        "أنس", "معاذ", "يحيى", "سامح", "وائل", "حاتم", "رامي", "باسل", "براء", "كنان", "غيث", "ساهر"
    ];

    private static readonly string[] ArabicFemaleNames =
    [
        "فاطمة", "عائشة", "مريم", "زينب", "خديجة", "ليلى", "نور", "ياسمين", "أميرة", "سلمى", "هناء", "ريم", "دينا",
        "سارة", "لجين", "لين", "جنى", "مايا", "رانيا", "لينا", "هالة", "ندى", "أسماء", "إيمان", "بشرى", "دلال",
        "غادة", "هند", "منى", "سهيلة"
    ];

    private static readonly Random random = new Random();

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
        RuleFor(e => e.Name, f => ArabicNameGenerator(f.Random.Number(3, 5)));

        RuleFor(e => e.NationalNumber, f => f.Random.String2(14, "0123456789").OrNull(f, 0.1f));
        RuleFor(e => e.AccountNumber, f => f.Random.String2(f.Random.Number(12, 14), "0123456789-").OrNull(f, 0.1f));


        RuleFor(e => e.Salary, f =>
        {
            decimal value = f.Random.Decimal(250m, 15000m);
            return value.OrNull(f, 0.1f);
        });
        RuleFor(e => e.FacultyId, f => f.Random.Int(1, 2).OrNull(f, 0.2f));
        RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 3).OrNull(f, 0.2f));
        RuleFor(e => e.JobTitle, f => f.Random.Byte(1, 3).OrNull(f, 0.1f));
        RuleFor(e => e.Status, f => f.Random.Byte(1, 3).OrNull(f, 0.1f));

        RuleFor(e => e.IsActive, f => f.Random.Bool(0.85f));
        RuleFor(e => e.CreatedBy, f => f.Random.Int(1, 100));
        RuleFor(e => e.CreatedAt, f => f.Date.Past(2));
    }

    string ArabicNameGenerator(int nameCount)
    {
        if (nameCount <= 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < nameCount; i++)
        {
            string selectedName;

            if (i == 0)
            {
                bool isMale = random.Next(2) == 0;

                selectedName = isMale
                    ? ArabicMaleNames[random.Next(ArabicMaleNames.Length)]
                    : ArabicFemaleNames[random.Next(ArabicFemaleNames.Length)];
            }
            else
            {
                // All subsequent names: strictly male
                selectedName = ArabicMaleNames[random.Next(ArabicMaleNames.Length)];
            }

            sb.Append(selectedName);

            // Adds space between names
            if (i < nameCount - 1)
            {
                sb.Append(' ');
            }
        }

        return sb.ToString();
    }


    /// <summary>
    /// Overwrites existing rules to force specific fields to NULL for testing edge cases.
    /// </summary>
    public IEntityFaker<Employee, EmployeeFields> WithNulls(EmployeeFields fields)
    {
        if (fields.HasFlag(EmployeeFields.Name)) RuleFor(e => e.Name, _ => null);
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
    public IEntityFaker<Employee, EmployeeFields> ForProperty<TProperty>(Expression<Func<Employee, TProperty>> property,
        TProperty value)
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
    public IEntityFaker<Employee, EmployeeFields> WithValue(EmployeeFields fields)
    {
        if (fields.HasFlag(EmployeeFields.NationalNumber))
            RuleFor(e => e.NationalNumber, f => f.Random.String2(14, "0123456789"));

        if (fields.HasFlag(EmployeeFields.AccountNumber))
            RuleFor(e => e.AccountNumber, f => f.Random.String2(f.Random.Number(12, 14), "0123456789-"));

        if (fields.HasFlag(EmployeeFields.Salary))
            RuleFor(e => e.Salary, f => f.Random.Decimal(250, 15_000));

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