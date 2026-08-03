using System;
using System.Collections.Generic;

namespace Benchmark.Database;

public class BulkImportBatch
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public string RawPayloadJson { get; set; } = string.Empty;

    public virtual ICollection<BulkImportResult> BulkImportResults { get; set; } = new List<BulkImportResult>();
}

public class BulkImportResult
{
    public int Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid Tracker { get; set; }
    public bool IsSuccess { get; set; }
    public int? EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ErrorTypeCode { get; set; }
    public string? ErrorMessage { get; set; }

    public virtual BulkImportBatch Batch { get; set; } = null!;
    public virtual Employee? Employee { get; set; }
}

public class Employee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = null!;
    public string? NationalNumber { get; set; }
    public string? AccountNumber { get; set; }
    public string? NationalNumberHash { get; set; }
    public string? AccountNumberHash { get; set; }
    public decimal? Salary { get; set; }
    public int? FacultyId { get; set; }
    public int? DepartmentId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public byte? Status { get; set; }

    public virtual ICollection<EmployeeNameToken> NameTokens { get; set; } = new List<EmployeeNameToken>();
    public virtual ICollection<BulkImportResult> BulkImportResults { get; set; } = new List<BulkImportResult>();
}

public class EmployeeNameToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHashed { get; set; } = null!;
    public byte N { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
