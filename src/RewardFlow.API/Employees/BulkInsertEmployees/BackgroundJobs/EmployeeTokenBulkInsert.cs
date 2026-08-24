using Microsoft.Data.SqlClient;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Employees.Data;
using RewardFlow_API.Employees.BulkInsertEmployees.Interfaces;
using System.Data;

namespace RewardFlow_API.Employees.BulkInsertEmployees.BackgroundJobs;

internal class EmployeeTokenBulkInsert(IConfiguration configuration, ILogger<EmployeeTokenBulkInsert> logger) : IBulkInserter<EmployeeNameToken>
{
    public async Task BulkInsertAsync(IEnumerable<EmployeeNameToken> tokens, int userId, Guid tenantId)
    {
        var dataTable = CreateDataTable(
            tokens,
            userId,
            tenantId);

        if (dataTable.Rows.Count == 0)
            return;

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        
        try
        {
            await connection.OpenAsync();

            using var bulkCopy = CreateBulkCopy(connection);

            await bulkCopy.WriteToServerAsync(dataTable);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to bulk insert employee name tokens");
        }
    }

    private static DataTable CreateDataTable(IEnumerable<EmployeeNameToken> tokens, int userId, Guid tenantId)
    {
        var dataTable = CreateDataTableSchema();

        LoadDataTable(dataTable, tokens, userId, tenantId);

        return dataTable;
    }

    private static DataTable CreateDataTableSchema()
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add("id", typeof(int));
        dataTable.Columns.Add("user_id", typeof(int));
        dataTable.Columns.Add("token_hashed", typeof(string));
        dataTable.Columns.Add("n", typeof(byte));
        dataTable.Columns.Add("employee_id", typeof(int));
        dataTable.Columns.Add("tenant_id", typeof(Guid));

        return dataTable;
    }

    private static void LoadDataTable(DataTable dataTable, IEnumerable<EmployeeNameToken> tokens, int userId,
        Guid tenantId)
    {
        foreach (var token in tokens)
        {
            dataTable.Rows.Add(
                token.Id,
                userId,
                token.TokenHashed,
                token.N,
                token.EmployeeId,
                tenantId);
        }
    }

    private SqlConnection CreateConnection()
    {
        var connectionString = AppConfiguration.Get("DefaultConnection");

        return new SqlConnection(connectionString);
    }

    private static SqlBulkCopy CreateBulkCopy(SqlConnection connection)
    {
        var bulkCopy = new SqlBulkCopy(connection) { DestinationTableName = "employee_name_tokens" };

        bulkCopy.ColumnMappings.Add("id", "id");
        bulkCopy.ColumnMappings.Add("user_id", "user_id");
        bulkCopy.ColumnMappings.Add("token_hashed", "token_hashed");
        bulkCopy.ColumnMappings.Add("n", "n");
        bulkCopy.ColumnMappings.Add("employee_id", "employee_id");
        bulkCopy.ColumnMappings.Add("tenant_id", "tenant_id");

        return bulkCopy;
    }
}