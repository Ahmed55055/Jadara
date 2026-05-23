using RewardFlow_API.Employees.BisunessRules;
using RewardFlow.TestUtilities.Extentions;

namespace RewardFlow_UnitTest.Employees.PropertyCases;

public class ValidEmployeePropertyCases
{
    private static string Repeat(char c, int len) => new string(c, len);

    private static string NationalNumGen(DateTime? birthDate = null)
    {
        birthDate ??= new Random().NextDate(new DateTime(1901, 1, 1), DateTime.Now);

        string nationalNum = BirthDateToNationalNum(birthDate.Value);

        nationalNum += Repeat('0', 7);

        return nationalNum;
    }

    /// <summary>
    /// Generates national number's first 7 digits based on birthdate
    /// </summary>
    /// <returns>First 7 digits of the national number</returns>
    private static string BirthDateToNationalNum(DateTime birthDate)
    {
        // First digit in national number is 2 for 20s century and 3 for 21
        string nationalNum;

        // [X]____________
        nationalNum = (Math.Floor((decimal)birthDate.Year / 100) + 1 == 21 ? 3 : 2).ToString();

        // X[XX]___________
        nationalNum += (birthDate.Year % 100).ToString();

        // XXX[XX]_________
        nationalNum += birthDate.Month.ToString("00");

        // XXXXX[XX]_______
        nationalNum += birthDate.Day.ToString("00");

        return nationalNum;
    }

    public readonly (string Value, string Description)[] Names =
    [
        (Repeat('a', EmployeeConstraints.Name.MinLength), "Min English"),
        (Repeat('a', EmployeeConstraints.Name.MaxLength), "Max English"),
        (Repeat('ع', EmployeeConstraints.Name.MinLength), "Min Arabic"),
        (Repeat('ع', EmployeeConstraints.Name.MaxLength), "Max Arabic"),
        ("Ahmed Ali", "With spaces")
    ];

    public readonly (decimal? value, string Description)[] Salaries =
    [
        (EmployeeConstraints.Salary.Min, $"Min salary"),
        (EmployeeConstraints.Salary.Max, $"Max salary"),
    ];

    public readonly (string? Value, string Description)[] NationalNums =
    [
        (NationalNumGen(EmployeeConstraints.NationalNum.OldestDate), "Oldest Birthdate"),
        (NationalNumGen(DateTime.Now.AddYears(-EmployeeConstraints.NationalNum.YoungestAge)), "Newest Birthdate")
    ];

    public readonly (string? value, string Description)[] AccountNums =
    [
        ("123-12345678", "Standard formate and length"),
        ("123456-12345678", "Max length"),
        ("123456789012", "No hyphens"),
        (null, "No value")
    ];

    public (int? value, string Description)[] ForeignKeysId =
    [
        (1, "Valid id"),
        (null, "No value")
    ];
}