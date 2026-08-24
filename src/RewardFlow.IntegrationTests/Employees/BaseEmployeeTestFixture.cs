using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees.Data;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Infrastructure.Requesters;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.UtilityClasses;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace RewardFlow.IntegrationTests.Employees;

public class BaseEmployeeTestFixture : IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;
    protected UserClient _userClient;
    protected EmployeeApi _employeeApi;
    protected ImportWaiter  _importWaiter;
    protected ITestOutputHelper _output;

    public BaseEmployeeTestFixture(TestWebApplicationFactory factory, ITestOutputHelper output = null)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
        _output = output;
    }

    public async Task InitializeAsync()
    {
        var faculties = await _dbUtility.Query<Faculty>().AnyAsync()
            ? await _dbUtility.Query<Faculty>().ToListAsync()
            : await AddFaculities();

        if (!await _dbUtility.Query<Department>().AnyAsync())
            await AddDepartments(faculties);

        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        _userClient = new UserClient(_factory, user);
        _employeeApi = new EmployeeApi(_userClient);
        _importWaiter = new ImportWaiter(_dbUtility, _userClient,_output);
    }

    public Task DisposeAsync() => Task.CompletedTask;

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

 public class ImportWaiter(DbUtility dbUtility, UserClient userClient, ITestOutputHelper _output)
    {
        private async Task WaitEmployeesProcessing(int expectedEmployeeCount, int waitSeconds)
        {
            var latestEmployeeCount = 0;

            await Waiter.Wait(async () =>
                {
                    var employees = await dbUtility.Query<Employee>()
                        .Where(e => e.CreatedBy == userClient.User.Id)
                        .Include(e => e.NameTokens)
                        .ToListAsync();

                    var currentCount = employees.Count;

                    var isDone = currentCount == expectedEmployeeCount;
                    var isProcessing = latestEmployeeCount > currentCount;

                    latestEmployeeCount = currentCount;

                    return new WaiterResult(isDone, isProcessing);
                }, waitSeconds, 5000,
                "Stable check limit reached. Employee Inserting Background job likely failed or stalled.",
                50);
        }

        private async Task WaitTokensProcessing(int waitSeconds)
        {
            var lastCompletedCount = 0;

            await Waiter.Wait(async () =>
            {
                var employees = await dbUtility.Query<Employee>()
                    .Where(e => e.CreatedBy == userClient.User.Id)
                    .Include(e => e.NameTokens)
                    .ToListAsync();

                // Count employees that has tokens that are fully processed (inserted AND have their tokens generated)
                int completedCount = employees.Count(e =>
                    e.NameTokens != null
                    && e.NameTokens.Count >=
                    (TotalTokensCount(e.Name.Length, 2) + TotalTokensCount(e.Name.Length, 3)) * 0.7);

                var isProcessing = lastCompletedCount > completedCount;
                lastCompletedCount = completedCount;

                return new WaiterResult(completedCount == employees.Count, isProcessing);
            }, waitSeconds, 20, "Stable check limit reached. Token Background job likely failed or stalled.", 500);
            return;

            int TotalTokensCount(int length, int tokenLength) { return Math.Max(0, length - tokenLength + 1); }
        }

        /// <summary>
        /// Waits for the background jobs to insert employees and generate their tokens.
        /// Employees are inserted first, then tokens are generated.
        /// </summary>
        public async Task WaitForImportProccessing(int expectedEmployeeCount)
        {
            var overAllStopwatch = Stopwatch.StartNew();

            var employeeStopwatch = Stopwatch.StartNew();
            await WaitEmployeesProcessing(expectedEmployeeCount, 30);
            employeeStopwatch.Stop();


            var tokensStopwatch = Stopwatch.StartNew();
            await WaitTokensProcessing(120);
            tokensStopwatch.Stop();

            overAllStopwatch.Stop();

            _output.WriteLine(
                $"✅ EMPLOYEE INSERTION TIME: Processed {expectedEmployeeCount} employees in {employeeStopwatch.Elapsed.TotalSeconds:F2} seconds.");
            _output.WriteLine(
                $"✅ TOKEN GENERATION TIME: Processed {expectedEmployeeCount} employees in {tokensStopwatch.Elapsed.TotalSeconds:F2} seconds.");
            _output.WriteLine(
                $"✅ OVERALL TIME: Processed {expectedEmployeeCount} employees in {overAllStopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }
    }