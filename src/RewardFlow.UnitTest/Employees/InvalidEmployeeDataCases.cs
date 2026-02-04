namespace RewardFlow_UnitTest.Employees;

public class InvalidEmployeeDataCases
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
    public static readonly (string? NationalNum, string Reason)[] NationalNums =
    [
        ("", "Empty string"),
        ("19123456789013", "Starts number less than 20"),
        ("31123456789013", "Starts number greater than 30"),
        ("2712345678901", "Too Short Only 13 digit"),
        ("301234567890134", "Too Long"),
        ("2612345678901a", "Contains Alphabets"),
        ("301 2345 6789 013", "14 number with spaces in between"),
        ("30 245 789 013", "14 letters counting spaces"),
        ("30-245-789-013", "14 letters counting hyphens")
    ];

    /// <summary>
    /// A collection of test cases representing invalid Account Number formats.
    /// </summary>
    /// <remarks>
    /// <b>Validation Strategy:</b> Empirical research on ~3,700 records suggests a 12–14 digit standard 
    /// with a hyphen after digits 3–5. However, due to legacy outliers (&lt;0.1%):
    /// <list type="bullet">
    /// <item>Length: Standard is 12–14, but one 15-digit record exists.</item>
    /// <item>Format: Some records have zero, two, or misplaced hyphens (e.g., after the 6th digit).</item>
    /// </list>
    /// To avoid blocking legacy data, validation is <b>permissive</b>: it rejects non-numeric 
    /// characters (except hyphens) and extreme lengths, prioritizing source consistency.
    /// </remarks>
    public static readonly (string? AccountNum, string Reason)[] AccountNums =
    [
        ("123-123456", "Too short (minimum 12 digits expected)"),
        ("12345678901234567", "Too long (maximum 16 characters allowed)"),
        ("1234-1234567a", "Contains alphabetic characters")
    ];
    
    /// <summary>
    /// A collection of test cases representing invalid Salary values.
    /// </summary>
    public static readonly (float? Salary, string Reason)[] Salary =
    [
        (-1f, "Negative Salary"),
        (0f, "Zero Salary"),
        (100_000f, "Salary exceeds maximum limit of 99,999 EGP")
    ];

    /// <summary>
    /// A collection of test cases representing invalid Foreign Key identifiers.
    /// </summary>
    public static readonly (int? ForeignKeyId, string Reason)[] ForeignKeysId =
    [
        (0, "Foreign keys Ids must be positive numbers")
    ];

}