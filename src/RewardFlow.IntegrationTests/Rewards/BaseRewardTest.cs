using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Employees;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards;
using RewardFlow_API.Rewards.Courses;
using RewardFlow_API.Rewards.Courses.Create;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.RewardManage.CreateReward;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.TestUtilities.DataGenerators;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Rewards;

public class BaseRewardTest: IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly DbUtility _dbUtility;
    protected UserClient _userClient;

    public BaseRewardTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _dbUtility = new DbUtility(_factory);
    }
    
    protected async Task<HttpResponseMessage> AddEmployeesAsync(int count, UserClient userClient, bool validateSuccess = true)
    {
        // Arrange
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, userClient.User.Id)
            .WithValue(EmployeeFields.Salary)
            .Generate(count);
        
        BulkRequest bulkRequest = BulkInsertEmployeeRequest(employees);
        
        // Act
        var response = await userClient.Client.PostAsJsonAsync(EmployeeApiPath.BulkInsertV2, bulkRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var batch = await response.Content.ReadFromJsonAsync<BulkImportBatch>();
        
        batch.Should().NotBeNull();
        batch.Id.Should().NotBe(Guid.Empty);
        
        return response;
    }
    
    protected BulkRequest BulkInsertEmployeeRequest(IEnumerable<Employee> employees)
    {
        return new BulkRequest(
            employees.Select(e => new BatchEmployee(
                    Tracker: Guid.NewGuid(),
                    Name: e.Name,
                    NationalNumber: e.NationalNumber,
                    AccountNumber: e.AccountNumber,
                    Salary: e.Salary))
                .ToList()
        );
    }

    protected async Task<UserClient> CreateUser()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        return new UserClient(_factory, user);
    }
    
    
    protected async Task<IEnumerable<CourseResponseDto>> InsertCourse(int count, UserClient userClient, bool validateSuccess = true)
    {
        var courses = TestDataGenerator.Course.Generate(count);
        var coursesDtos = new List<CourseResponseDto>();
        
        foreach (Course course in courses)
        {
            var request = new CourseRequestDto(course.Name, course.Code, course.IsTheoretical, course.IsPractical,
                course.SubjectPrice);
            
            var response = await _userClient.Client.PostAsJsonAsync(CourseApiPath.Courses, request);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<CourseResponseDto>();
            dto.Should().NotBeNull();
            dto.Id.Should().BeGreaterThan(0);
            coursesDtos.Add(dto);
        }

        return coursesDtos;
    }
    
    protected async Task<int> CreateReward()
    {
        const string rewardCreatingErrorMessage =
            "Error while creating session reward";

        var request = new CreateSessionsReward.Request(
            "جلسات شفوية الفصل الدراسي الاول 2026",
            "1984C1",
            2026, 
            1, 
            .04m);

        var response = await _userClient.Client.PostAsJsonAsync(RewardApiPath.SessionRewards, request);

        response.Should().NotBeNull(rewardCreatingErrorMessage);
        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            rewardCreatingErrorMessage);

        return await response.Content.ReadFromJsonAsync<int>();
    }
    
    protected async Task<List<Employee>> AddEmployeesAndGetEmployeesAsync(
        int count,
        decimal? salary = null)
    {
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .WithValue(EmployeeFields.Salary)
            .Generate(count)
            .ToList();

        if (salary.HasValue)
        {
            foreach (var employee in employees)
            {
                employee.Salary = salary.Value;
            }
        }

        var bulkRequest = BulkInsertEmployeeRequest(employees);

        var response = await _userClient.Client.PostAsJsonAsync(
            EmployeeApiPath.BulkInsertV2,
            bulkRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var  batch = await response.Content.ReadFromJsonAsync<Guid>();
        
        await WaitBatchProccessing(batch);
        
        var persistedEmployees = await _dbUtility
            .Query<Employee>()
            .Where(e => e.CreatedBy == _userClient.User.Id)
            .ToListAsync();

        persistedEmployees.Should().HaveCount(count);

        return persistedEmployees;
    }

    private async Task WaitBatchProccessing(Guid batchId)
    {
        const int retries = 5;
        int retry = 0;
        const int waitTimeInSeconds = 2;
        BulkImportBatch? batch = null;
        
        while (retry <= retries )
        {
            batch = await _dbUtility
                .Query<BulkImportBatch>()
                .Where(b => b.Id == batchId )
                .FirstOrDefaultAsync();

            batch.Should().NotBeNull();
            
            if (batch.Status == "Completed")
                return;

            Task.Delay(waitTimeInSeconds * 1000).Wait();
            retry++;
        }

        batch!.Status.Should().Be("Completed","Failed at employees arranging process and never added");
    }
}