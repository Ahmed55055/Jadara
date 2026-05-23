using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.EmployeeDb
{
    /// <inheritdoc />
    public partial class SyncEmployeeChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Salary_NonNegative",
                table: "employees");
            
            migrationBuilder.AlterColumn<decimal>(
                name: "salary",
                schema: "dbo",
                table: "employees",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
            
            migrationBuilder.AddCheckConstraint(
                    name: "CHK_Salary_NonNegative",
                    table: "employees",
                    sql: "[salary] >= 0 OR [salary] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Salary_NonNegative",
                table: "employees");
            
            migrationBuilder.AlterColumn<float>(
                name: "salary",
                schema: "dbo",
                table: "employees",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
            
            migrationBuilder.AddCheckConstraint(
                name: "CHK_Salary_NonNegative",
                table: "employees",
                sql: "[salary] >= 0 OR [salary] IS NULL");
        }
    }
}
