using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.User.Data;
using RewardFlow_API.Rewards.Data;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Courses;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Users;

namespace RewardFlow.TestUtilities.DataGenerators;

public class TestDataGenerator
{
    /// <summary>
    /// Gets a new instance of faker / generator for creating test user data.
    /// </summary>
    public static IEntityFaker<User,UserFields> User => new UserFaker();

    /// <summary>
    /// Gets a new instance of faker / generator for creating test employee data.
    /// </summary>
    public static IEntityFaker<Employee,EmployeeFields> Employee => new EmployeeFaker();
    
    public static IEntityFaker<Course,CourseFields> Course => new CourseFaker();
}