using EntityFramework.Exceptions.Common;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Reward_Flow_v2.Common.Enums;
using Reward_Flow_v2.Employees.CreateEmployee;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net;

namespace Reward_Flow_v2.Employees.BulkInsertEmployees;

public record SuccessfulRecord(Guid Tracker, int EmployeeId, string Name);

public enum ErrorTypes
{
    InvalidName,
    DuplicateNationalNumber,
    DuplicateAccountNumber,
    DatabaseConflict,
    Unexpected
}

public record BulkError(Guid Tracker, ErrorTypes ErrorStatusCode, string Message);

public record BatchSummary(int TotalRecords, int SuccessfulRecords, int FailedRecords);

public record BulkResponse(BatchSummary Summary, SuccessfulRecord[] InsertedRecords, BulkError[] Errors);

public record BulkRequest(List<BatchEmployee> Employees);