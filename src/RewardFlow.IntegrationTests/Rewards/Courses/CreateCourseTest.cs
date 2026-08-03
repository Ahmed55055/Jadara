using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardFlow_API.Rewards.Courses;
using RewardFlow_API.Rewards.Data;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Rewards;
using RewardFlow.TestUtilities.DataGenerators;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Rewards.Courses;

public class CreateCourseTest(TestWebApplicationFactory _factory)
    : BaseCourseTest(_factory), IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();

        await _dbUtility.InsertAsync(user);

        _userClient = new UserClient(_factory, user);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateCourse_WithAllValidValues_ShouldReturnCreated()
    {
        var course = TestDataGenerator.Course.Generate();

        var request = new CourseRequestDto(
            course.Name,
            course.Code,
            course.IsTheoretical,
            course.IsPractical,
            course.SubjectPrice);

        var response = await _userClient.Client.PostAsJsonAsync(CourseApiPath.Courses, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCourse = await response.Content.ReadFromJsonAsync<CourseResponseDto>();

        createdCourse.Should().NotBeNull();
        createdCourse.Name.Should().Be(request.Name);
        createdCourse.Code.Should().Be(request.Code);
        createdCourse.IsTheoretical.Should().Be(request.IsTheoretical);
        createdCourse.IsPractical.Should().Be(request.IsPractical);
        createdCourse.SubjectPrice.Should().Be(request.SubjectPrice);

        var savedCourse = await _dbUtility.Query<Course>()
            .SingleAsync(c => c.Id == createdCourse.Id);

        savedCourse.Name.Should().Be(request.Name);
        savedCourse.Code.Should().Be(request.Code);
        savedCourse.IsTheoretical.Should().Be(request.IsTheoretical);
        savedCourse.IsPractical.Should().Be(request.IsPractical);
        savedCourse.SubjectPrice.Should().Be(request.SubjectPrice);
    }

    [Fact]
    public async Task CreateCourse_WithOnlyName_ShouldReturnCreated()
    {
        var course = TestDataGenerator.Course.Generate();

        var request = new CourseRequestDto(
            course.Name,
            default!,
            default,
            default,
            default);

        var response = await _userClient.Client.PostAsJsonAsync(
            CourseApiPath.Courses,
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCourse =
            await response.Content.ReadFromJsonAsync<CourseResponseDto>();

        createdCourse.Should().NotBeNull();
        createdCourse.Name.Should().Be(request.Name);
        createdCourse.Code.Should().Be(request.Code);
        createdCourse.IsTheoretical.Should().Be(request.IsTheoretical);
        createdCourse.IsPractical.Should().Be(request.IsPractical);
        createdCourse.SubjectPrice.Should().Be(request.SubjectPrice);

        var savedCourse = await _dbUtility.Query<Course>()
            .SingleAsync(c => c.Id == createdCourse.Id);

        savedCourse.Name.Should().Be(request.Name);
        savedCourse.Code.Should().Be(request.Code);
        savedCourse.IsTheoretical.Should().Be(request.IsTheoretical);
        savedCourse.IsPractical.Should().Be(request.IsPractical);
        savedCourse.SubjectPrice.Should().Be(request.SubjectPrice);
    }

    [Fact]
    public async Task CreateCourse_WithEmptyName_ShouldReturnBadRequest()
    {
        var course = TestDataGenerator.Course.Generate();

        var request = new CourseRequestDto(
            string.Empty,
            course.Code,
            course.IsTheoretical,
            course.IsPractical,
            course.SubjectPrice);

        var response = await _userClient.Client.PostAsJsonAsync(
            CourseApiPath.Courses,
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCourse_WithNullName_ShouldReturnBadRequest()
    {
        var course = TestDataGenerator.Course.Generate();

        var request = new CourseRequestDto(
            null!,
            course.Code,
            course.IsTheoretical,
            course.IsPractical,
            course.SubjectPrice);

        var response = await _userClient.Client.PostAsJsonAsync(
            CourseApiPath.Courses,
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}