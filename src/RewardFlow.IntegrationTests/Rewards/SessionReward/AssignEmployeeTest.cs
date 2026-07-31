using FluentAssertions;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using RewardFlow_API.Rewards.Courses;
using RewardFlow_API.Rewards.SessionsReward.EndPoints.SessionReward.CourseAssignments;
using RewardFlow.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Rewards;

public class AssignEmployeeTest(TestWebApplicationFactory _factory) : BaseRewardTest(_factory), IAsyncLifetime
{
    private int _rewardId;
    private CourseResponseDto _termCourse;
    private List<Employee> _employees;

    private List<int> _employeesIds => _employees.Select(e => e.EmployeeId).ToList();


    public async Task InitializeAsync()
    {
        _userClient = await CreateUser();

        _rewardId = await CreateReward();

        _termCourse = (await InsertCourse(count: 1, userClient: _userClient)).Single();

        _employees = await AddEmployeesAndGetEmployeesAsync(5);
    }

    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task AssignEmployee_WithValidValues_ShouldReturnCreated()
    {
        // Arrange
        var request = CreateAssignmentRequest(_rewardId, _termCourse.Id);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(RewardApiPath.CourseAssignments, request);

        // Assert
        await AssertAssignmentCreated(response, _rewardId, _termCourse.Id, _employees);
    }


    [Fact]
    public async Task AssignEmployee_WithZeroSalary_ShouldReturnCreated()
    {
        // Arrange
        foreach (var employee in _employees)
        {
            employee.Salary = null;
        }

        var request = CreateAssignmentRequest(
            _rewardId,
            _termCourse.Id);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertAssignmentCreated(
            response,
            _rewardId,
            _termCourse.Id,
            _employees);
    }


    [Fact]
    public async Task AssignEmployee_WithNonExistentReward_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateAssignmentRequest(
            int.MaxValue,
            _termCourse.Id);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task AssignEmployee_WithNonExistentTermCourse_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateAssignmentRequest(
            _rewardId,
            int.MaxValue);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task AssignEmployee_WhenMainEmployeeIsNotInEmployeesIds_ShouldReturnValidationError()
    {
        // Arrange
        var request = new AddCourseAssignmentDto
        {
            RewardId = _rewardId,
            TermCourseId = _termCourse.Id,
            NumberOfStudents = 100,
            MainEmployeeId = _employees.First().EmployeeId,
            EmployeesIds = _employeesIds
                .Skip(1)
                .ToList()
        };

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertValidationError(response, "MainEmployeeId");
    }


    [Fact]
    public async Task AssignEmployee_WithEmptyEmployeesIds_ShouldReturnValidationError()
    {
        // Arrange
        var request = new AddCourseAssignmentDto
        {
            RewardId = _rewardId,
            TermCourseId = _termCourse.Id,
            NumberOfStudents = 100,
            MainEmployeeId = _employees.First().EmployeeId,
            EmployeesIds = []
        };

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertValidationError(response, "EmployeesIds");
    }


    [Fact]
    public async Task AssignEmployee_WithSameEmployeeMultipleTimesForSameTermCourse_ShouldReturnValidationError()
    {
        // Arrange
        var request = CreateAssignmentRequest(
            _rewardId,
            _termCourse.Id);

        // Act
        var firstResponse = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        var secondResponse = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertAssignmentCreated(
            firstResponse,
            _rewardId,
            _termCourse.Id,
            _employees);

        await AssertValidationError(secondResponse);
    }


    [Fact]
    public async Task AssignEmployee_WithSameEmployeesForDifferentTermCoursesWithinSameReward_ShouldReturnCreated()
    {
        // Arrange
        var secondTermCourse = (await InsertCourse(
            count: 1,
            userClient: _userClient))
            .Single();

        var firstRequest = CreateAssignmentRequest(
            _rewardId,
            _termCourse.Id);

        var secondRequest = CreateAssignmentRequest(
            _rewardId,
            secondTermCourse.Id);

        // Act
        var firstResponse = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            firstRequest);

        var secondResponse = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            secondRequest);

        // Assert
        await AssertAssignmentCreated(
            firstResponse,
            _rewardId,
            _termCourse.Id,
            _employees);

        await AssertAssignmentCreated(
            secondResponse,
            _rewardId,
            secondTermCourse.Id,
            _employees);
    }


    [Fact]
    public async Task AssignEmployee_WithFiveEmployeesForSameTermCourse_ShouldReturnCreated()
    {
        // Arrange
        var request = CreateAssignmentRequest(
            _rewardId,
            _termCourse.Id);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertAssignmentCreated(
            response,
            _rewardId,
            _termCourse.Id,
            _employees);
    }


    [Fact]
    public async Task AssignEmployee_WithMoreThanFiveEmployeesForSameTermCourse_ShouldReturnValidationError()
    {
        // Arrange
        var employees = await AddEmployeesAndGetEmployeesAsync(6);

        var request = new AddCourseAssignmentDto
        {
            RewardId = _rewardId,
            TermCourseId = _termCourse.Id,
            NumberOfStudents = 100,
            MainEmployeeId = employees.First().EmployeeId,
            EmployeesIds = employees
                .Select(e => e.EmployeeId)
                .ToList()
        };

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(
            RewardApiPath.CourseAssignments,
            request);

        // Assert
        await AssertValidationError(response);
    }


    private AddCourseAssignmentDto CreateAssignmentRequest(
        int rewardId,
        int termCourseId)
    {
        return new AddCourseAssignmentDto
        {
            RewardId = rewardId,
            TermCourseId = termCourseId,
            NumberOfStudents = 100,
            MainEmployeeId = _employees.First().EmployeeId,
            EmployeesIds = _employeesIds
        };
    }


    private static async Task AssertAssignmentCreated(HttpResponseMessage response, int rewardId, int termCourseId,
        List<Employee> employees)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content
            .ReadFromJsonAsync<CourseAssignmentDto>();

        result.Should().NotBeNull();

        result.CourseAssignmentId
            .Should()
            .BeGreaterThan(0);

        result.RewardId
            .Should()
            .Be(rewardId);

        result.TermCourseId
            .Should()
            .Be(termCourseId);

        result.AssignedEmployees
            .Should()
            .HaveCount(employees.Count);
    }


    private static async Task AssertValidationError(
        HttpResponseMessage response,
        string? expectedMessage = null)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        if (expectedMessage is not null)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync();

            errorContent
                .Should()
                .ContainEquivalentOf(expectedMessage);
        }
    }

}