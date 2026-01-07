using Reward_Flow_v2.Employees.Data;
using Xunit;

namespace RewardFlow.IntegrationTests.Infrastructure;

public class EmployeeTestFixture : TestWebApplicationFactory, IAsyncLifetime
{
    private DbUtility dbUtility;

    public async Task InitializeAsync()
    {
        await base.InitializeAsync();

        dbUtility = new DbUtility(this);

        var faculties = await dbUtility.AnyAsync<Faculty>()
            ? await dbUtility.GetAllAsync<Faculty>()
            : await AddFaculities();

        if (!await dbUtility.AnyAsync<Department>())
            await AddDepartments(faculties);
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        await base.DisposeAsync();
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
        await dbUtility.InsertRangeAsync(faculities);
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

        await dbUtility.InsertRangeAsync(departments);
        return departments;
    }
}