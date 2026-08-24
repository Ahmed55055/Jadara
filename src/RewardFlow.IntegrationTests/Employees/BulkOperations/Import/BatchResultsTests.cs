using FluentAssertions;
using RewardFlow.IntegrationTests.Infrastructure;
using RewardFlow.TestUtilities.DataGenerators;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace RewardFlow.IntegrationTests.Employees.BulkOperations.Import;

public class BatchResultsTests(TestWebApplicationFactory factory, ITestOutputHelper output) : BaseEmployeeTestFixture(factory,output), IAsyncLifetime
{
    [Fact]
    public async Task GetBatchResult_WithRandomBatchId_ShouldReturnNotFound()
    {
        // Arrange
        // adding employees to make sure even when there is already a batch it won't return data because it's correct
        // not because it's filtered by the tenant id
        const int count = 20;
        var employees = TestDataGenerator.Employee
            .ForProperty(e => e.CreatedBy, _userClient.User.Id)
            .Generate(count);

        await _employeeApi.ImportAsync(employees);
        await _importWaiter.WaitForImportProccessing(count);
        
        var batchId = Guid.NewGuid();

        // Act
        var result = await _employeeApi.GetBatchResult(batchId, HttpStatusCode.NotFound);

        // Assert
        result.Should().BeNull();
    }
}