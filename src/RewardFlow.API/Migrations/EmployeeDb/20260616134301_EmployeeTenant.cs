using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.EmployeeDb
{
    /// <inheritdoc />
    public partial class EmployeeTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_name_tokens_employees_employee_id",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.DropTable(
                name: "BulkImportBatches",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BulkImportResults",
                schema: "dbo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                schema: "dbo",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employee_name_tokens_employee_id",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.DropIndex(
                name: "IX_employee_name_tokens_user_id_token_hashed",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Salary_NonNegative",
                table: "employees",
                schema: "dbo");
            
            migrationBuilder.AlterColumn<float>(
                name: "salary",
                schema: "dbo",
                table: "employees",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "dbo",
                table: "employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "dbo",
                table: "employee_name_tokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                schema: "dbo",
                table: "employees",
                columns: new[] { "tenant_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_tenant_id",
                schema: "dbo",
                table: "employees",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_tenant_id",
                schema: "dbo",
                table: "employee_name_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_tenant_id_token_hashed",
                schema: "dbo",
                table: "employee_name_tokens",
                columns: new[] { "tenant_id", "token_hashed" });
            
            migrationBuilder.CreateCheckConstraint(
                name: "CHK_Salary_NonNegative",
                table: "employees",
                schema: "dbo",
                sql: "[salary] >= 0 OR [salary] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                schema: "dbo",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_tenant_id",
                schema: "dbo",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employee_name_tokens_tenant_id",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.DropIndex(
                name: "IX_employee_name_tokens_tenant_id_token_hashed",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "dbo",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "dbo",
                table: "employee_name_tokens");

            migrationBuilder.AlterColumn<decimal>(
                name: "salary",
                schema: "dbo",
                table: "employees",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                schema: "dbo",
                table: "employees",
                column: "employee_id");

            migrationBuilder.CreateTable(
                name: "BulkImportBatches",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
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
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tracker = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkImportResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_employee_id",
                schema: "dbo",
                table: "employee_name_tokens",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_name_tokens_user_id_token_hashed",
                schema: "dbo",
                table: "employee_name_tokens",
                columns: new[] { "user_id", "token_hashed" });

            migrationBuilder.AddForeignKey(
                name: "FK_employee_name_tokens_employees_employee_id",
                schema: "dbo",
                table: "employee_name_tokens",
                column: "employee_id",
                principalSchema: "dbo",
                principalTable: "employees",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
