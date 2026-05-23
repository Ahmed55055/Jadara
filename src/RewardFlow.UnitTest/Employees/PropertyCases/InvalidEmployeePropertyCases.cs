namespace RewardFlow_UnitTest.Employees.PropertyCases;

public class InvalidEmployeePropertyCases
{
    /// <summary>
    /// A collection of test cases representing invalid Egyptian National ID formats.
    /// Valid IDs must:
    /// <list type="bullet">
    /// <item>Start with '2' (1900-1999) or '3' (2000-2099).</item>
    /// <item>Be exactly 14 digits long.</item>
    /// <item>Contain no spaces, hyphens, or alphabetic characters.</item>
    /// </list>
    /// Note: IDs starting below 20 (1800s) are considered out of scope/invalid for this system.
    /// </summary>
    public readonly (string? NationalNum, string Reason)[] NationalNums =
    [
        ("", "Empty string"),
        ("19123456789013", "Starts number less than 20"),
        ("31123456789013", "Starts number greater than 30"),
        ("2712345678901", "Too Short Only 13 digit"),
        ("301234567890134", "Too Long"),
        ("2612345678901a", "Contains Alphabets"),
        ("301 2345 6789 013", "14 digits with spaces in between"),
        ("30 245 789 013", "14 letters counting spaces"),
        ("30-245-789-013", "14 letters counting hyphens")
    ];

    /// <summary>
    /// A collection of test cases representing invalid Account Number formats.
    /// </summary>
    /// <remarks>
    /// <b>Validation Strategy:</b> Empirical research on ~3,700 records suggests a 12–14 digit standard 
    /// counting a hyphen after digits 3–5. However, due to legacy outliers (&lt;0.1%):
    /// <list type="bullet">
    /// <item>Length: Standard is 12–14, but one 15-digit record exists.</item>
    /// <item>Format: Some records have zero, two, or misplaced hyphens (e.g., after the 6th digit).</item>
    /// </list>
    /// To avoid blocking legacy data, validation is <b>permissive</b>: it rejects non-numeric 
    /// characters (except hyphens) and extreme lengths, prioritizing source consistency.
    /// </remarks>
    public readonly (string? AccountNum, string Reason)[] AccountNums =
    [
        ("123-1234567", "Too short (minimum 12 digits expected)"),
        ("12345678901234567", "Too long (maximum 16 characters allowed)"),
        ("1234-1234567a", "Contains alphabetic characters"),
        ("-123456789012", "Leading Hyphen"),
        ("123456789012-", "Trailing Hyphen"),
        ("123--456789012", "Double Hyphen"),
        ("123-!2345678", "Special Characters")
    ];
    
    /// <summary>
    /// A collection of test cases representing invalid Salary values.
    /// </summary>
    public readonly (decimal? Salary, string Reason)[] Salary =
    [
        (-1m, "Negative Salary"),
        (0m, "Zero Salary"),
        (100_000m, "Salary exceeds maximum limit of 99,999 EGP")
    ];

    /// <summary>
    /// A collection of test cases representing invalid Foreign Key identifiers.
    /// </summary>
    public readonly (int? ForeignKeyId, string Reason)[] ForeignKeysId =
    [
        (0, "Foreign keys Ids must be positive numbers")
    ];

    public readonly IEnumerable<(string? value, string reason)> Name =
    [
        ("", "Name is empty"),
        (null, "Name is null"),
        ("   ", "contains only spaces"),
        ("John_&Doe", "contains special characters"),
        ("John1234", "contains numbers"),
        (new string('a', 256), "Name exceeds maximum length of 256 characters")
    ];
}