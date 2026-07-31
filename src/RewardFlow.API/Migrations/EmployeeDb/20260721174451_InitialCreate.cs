using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RewardFlow_API.Migrations.EmployeeDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "BulkImportBatches",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BulkImportResults",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tracker = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImportResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employee_status",
                schema: "dbo",
                columns: table => new
                {
                    status_id = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_status", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                schema: "dbo",
                columns: table => new
                {
                    faculty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    is_default = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.faculty_id);
                });

            migrationBuilder.CreateTable(
                name: "job_titles",
                schema: "dbo",
                columns: table => new
                {
                    job_title_id = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_titles", x => x.job_title_id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "dbo",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    faculty_id = table.Column<int>(type: "int", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.department_id);
                    table.ForeignKey(
                        name: "FK_departments_faculties_faculty_id",
                        column: x => x.faculty_id,
                        principalSchema: "dbo",
                        principalTable: "faculties",
                        principalColumn: "faculty_id");
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "dbo",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    national_number = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    account_number = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    national_number_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    account_number_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    faculty_id = table.Column<int>(type: "int", nullable: true),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    job_title = table.Column<byte>(type: "tinyint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    status = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => new { x.tenant_id, x.employee_id });
                    table.CheckConstraint("CHK_Salary_NonNegative", "[salary] >= 0 OR [salary] IS NULL");
                    table.ForeignKey(
                        name: "FK_employees_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "dbo",
                        principalTable: "departments",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_employees_employee_status_status",
                        column: x => x.status,
                        principalSchema: "dbo",
                        principalTable: "employee_status",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "FK_employees_faculties_faculty_id",
                        column: x => x.faculty_id,
                        principalSchema: "dbo",
                        principalTable: "faculties",
                        principalColumn: "faculty_id");
                    table.ForeignKey(
                        name: "FK_employees_job_titles_job_title",
                        column: x => x.job_title,
                        principalSchema: "dbo",
                        principalTable: "job_titles",
                        principalColumn: "job_title_id");
                });

            migrationBuilder.CreateTable(
                name: "employee_name_tokens",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hashed = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    n = table.Column<byte>(type: "tinyint", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_name_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_name_tokens_employees_tenant_id_employee_id",
                        columns: x => new { x.tenant_id, x.employee_id },
                        principalSchema: "dbo",
                        principalTable: "employees",
                        principalColumns: new[] { "tenant_id", "employee_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "employee_status",
                columns: new[] { "status_id", "description", "name" },
                values: new object[,]
                {
                    { (byte)1, "Active employee", "Active" },
                    { (byte)2, "Inactive employee", "Inactive" },
                    { (byte)3, "Suspended employee", "Suspended" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "job_titles",
                columns: new[] { "job_title_id", "description", "name" },
                values: new object[,]
                {
                    { (byte)1, "Regular employee", "Employees" },
                    { (byte)2, "Professor", "Professor" },
                    { (byte)3, "Teaching Assistant or Graduate Assistant", "Teaching Assistant / Graduate Assistant" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_departments_faculty_id",
                schema: "dbo",
                table: "departments",
                column: "faculty_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_tenant_id",
                schema: "dbo",
                table: "employee_name_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_tenant_id_employee_id",
                schema: "dbo",
                table: "employee_name_tokens",
                columns: new[] { "tenant_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_tenant_id_token_hashed",
                schema: "dbo",
                table: "employee_name_tokens",
                columns: new[] { "tenant_id", "token_hashed" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_CreatedBy_AccountNumberHash",
                schema: "dbo",
                table: "employees",
                columns: new[] { "created_by", "account_number_hash" },
                unique: true,
                filter: "[account_number_hash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_CreatedBy_NationalNumberHash",
                schema: "dbo",
                table: "employees",
                columns: new[] { "created_by", "national_number_hash" },
                unique: true,
                filter: "[national_number_hash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_department_id",
                schema: "dbo",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_faculty_id",
                schema: "dbo",
                table: "employees",
                column: "faculty_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_job_title",
                schema: "dbo",
                table: "employees",
                column: "job_title");

            migrationBuilder.CreateIndex(
                name: "IX_employees_status",
                schema: "dbo",
                table: "employees",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_employees_tenant_id",
                schema: "dbo",
                table: "employees",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Faculties_CreatedBy_Name",
                schema: "dbo",
                table: "faculties",
                columns: new[] { "created_by", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__job_title__72E12F1B42005F0D",
                schema: "dbo",
                table: "job_titles",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkImportBatches",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BulkImportResults",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "employee_name_tokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "employee_status",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "job_titles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "faculties",
                schema: "dbo");
        }
    }
}
