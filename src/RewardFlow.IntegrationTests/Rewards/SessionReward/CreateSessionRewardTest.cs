using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.SessionsReward.CreateReward;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.IntegrationTests.Infrastructure.Rewards;
using RewardFlow.TestUtilities.DataGenerators;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RewardFlow.IntegrationTests.Rewards.SessionReward;

//[Collection("SessionRewardTests")]
public class CreateSessionRewardTest(TestWebApplicationFactory factory) : BaseRewardTest(factory), IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var user = TestDataGenerator.User.Generate();
        await _dbUtility.InsertAsync(user);
        user.Id.Should().BeGreaterThan(1);
        _factory.Should().NotBeNull();
        _userClient = new UserClient(_factory, user);
        _userClient.Authanticate();
    }

    public Task DisposeAsync() => Task.CompletedTask;


    [Theory]
    [InlineData(null, null, (short)2026, (byte)1, .04d)]
    [InlineData("جلسات شفوية الفصل الدراسي الاول 2026", "1984C1", (short)2026, (byte)1, .04d)]
    public async Task CreateReward_WithValidCredentials_ShouldReturnCreated(string? Name, string? Code, short Year,
        byte Semester, decimal Percentage)
    {
        // Arrange
        var request = new CreateSessionsReward.Request(Name, Code, Year, Semester, Percentage);

        // Act
        var response = await _userClient.Client.PostAsJsonAsync(RewardApiPath.CreateSessionsReward, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await response.Content.ReadFromJsonAsync<int>();
        id.Should().BeGreaterThanOrEqualTo(1);

        var reward = await GetSessionRewardById(id);

        CompareWithRequest(reward, request);
    }

    [Fact]
    public async Task CreateReward_Duplicate_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateSessionsReward.Request(null, null, 2026, 1, .04m);
        var baseResponse = await _userClient.Client.PostAsJsonAsync(RewardApiPath.CreateSessionsReward, request);

        // Act
        var duplicateResponse = await _userClient.Client.PostAsJsonAsync(RewardApiPath.CreateSessionsReward, request);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var baseId = await baseResponse.Content.ReadFromJsonAsync<int>();
        var duplicateid = await duplicateResponse.Content.ReadFromJsonAsync<int>();

        duplicateid.Should().BeGreaterThanOrEqualTo(1);
        duplicateid.Should().NotBe(baseId);

        SessionRewardEntity? duplicateReward = await GetSessionRewardById(duplicateid);
        CompareWithRequest(duplicateReward, request);

        // Checks if maybe the duplicate overwritten the original record 
        SessionRewardEntity? baseReward = await GetSessionRewardById(duplicateid);
        CompareWithRequest(baseReward, request);
    }

    private async Task<SessionRewardEntity?> GetSessionRewardById(int id)
    {
        var reward = await _dbUtility.Set<SessionRewardEntity>()
            .Include(r => r.Reward)
            .FirstOrDefaultAsync(r => r.Id == id);
        return reward;
    }

    private void CompareWithRequest(SessionRewardEntity? reward, CreateSessionsReward.Request request)
    {
        reward.Should().NotBeNull();

        reward.Reward.Name.Should().Be(request.Name);
        reward.Reward.Code.Should().Be(request.Code);
        reward.Year.Should().Be(request.Year);
        reward.semester.Should().Be(request.Semester);
        reward.Percentage.Should().Be(request.Percentage);

        reward.Reward.CreatedBy.Should().Be(_userClient.User.Id);
    }
}