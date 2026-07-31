using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace RewardFlow.IntegrationTests.Employees;

public class BaseEmployeeTestFixture: IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;


    public BaseEmployeeTestFixture(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }
    
    public async Task InitializeAsync()
    {
        var faculties = await _dbUtility.Query<Faculty>().AnyAsync()
            ? await _dbUtility.Query<Faculty>().ToListAsync()
            : await AddFaculities();

        if (!await _dbUtility.Query<Department>().AnyAsync())
            await AddDepartments(faculties);
    }

    private async Task<List<Faculty>> AddFaculities()
    {
        var faculities = new List<Faculty>()
        {
            new Faculty { Name = "Faculty of Engineering", IsDefault = true },
            new Faculty { Name = "Faculty of Business", IsDefault = true },
            new Faculty { Name = "Faculty of Science", IsDefault = true },
            new Faculty { Name = "Faculty of Arts", IsDefault = true }
        };
        await _dbUtility.InsertRangeAsync(faculities);
        return faculities;
    }

    private async Task<List<Department>> AddDepartments(List<Faculty> faculities)
    {
        var departments = new List<Department>
        {
            new Department { Name = "Education Technology", IsDefault = false, Faculty = faculities[0] },
            new Department { Name = "Human Resources", IsDefault = false, Faculty = faculities[1] },
            new Department { Name = "Finance", IsDefault = false, Faculty = faculities[1] },
            new Department { Name = "Information Technology", IsDefault = false, Faculty = faculities[0] },
            new Department { Name = "Marketing", IsDefault = false, Faculty = faculities[1] },
            new Department { Name = "Operations", IsDefault = true, Faculty = faculities[1] }
        };

        await _dbUtility.InsertRangeAsync(departments);
        return departments;
    }
}