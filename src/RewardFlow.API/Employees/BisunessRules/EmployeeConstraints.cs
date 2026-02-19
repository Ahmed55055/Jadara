namespace RewardFlow_API.Employees.BisunessRules;

public static class EmployeeConstraints
{
    public static class Name
    {
        public const int MinLength = 3;
        public const int MaxLength = 255;
    }

    public static class Salary
    {
        public const float Min = 0f;
        public const float Max = 100000.00f;
    }

    public static class NationalNum
    {
        // Using DateTime for the oldest date and int for age logic
        public static readonly DateTime OldestDate = new DateTime(1901, 1, 1);
        public const int YoungestAge = 18; 
    }
}