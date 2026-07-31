using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.EmployeeDb
{
    /// <inheritdoc />
    public partial class BulkImportBatch_AddTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "dbo",
                table: "BulkImportBatches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BulkImportBatches_tenant_id",
                schema: "dbo",
                table: "BulkImportBatches",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BulkImportBatches_tenant_id",
                schema: "dbo",
                table: "BulkImportBatches");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "dbo",
                table: "BulkImportBatches");
        }
    }
}
