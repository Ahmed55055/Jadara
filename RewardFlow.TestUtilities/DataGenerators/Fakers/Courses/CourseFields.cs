namespace RewardFlow.TestUtilities.DataGenerators.Fakers.Courses;

[Flags]
public enum CourseFields
{
    None = 0,
    Name = 1,
    Code = 2,
    IsTheoretical = 4,
    IsPractical = 8,
    SubjectPrice = 16
}