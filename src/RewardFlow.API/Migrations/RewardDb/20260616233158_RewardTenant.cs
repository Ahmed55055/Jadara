using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.RewardDb
{
    /// <inheritdoc />
    public partial class RewardTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DbReward");

            migrationBuilder.RenameTable(
                name: "subjects",
                schema: "dbo",
                newName: "subjects",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "subject_session_rewards",
                schema: "dbo",
                newName: "subject_session_rewards",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "subject_semesters",
                schema: "dbo",
                newName: "subject_semesters",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "session_rewards",
                schema: "dbo",
                newName: "session_rewards",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "rewards",
                schema: "dbo",
                newName: "rewards",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "employee_session_rewards",
                schema: "dbo",
                newName: "employee_session_rewards",
                newSchema: "DbReward");

            migrationBuilder.RenameTable(
                name: "employee_rewards",
                schema: "dbo",
                newName: "employee_rewards",
                newSchema: "DbReward");

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "subjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "subject_semesters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "DbReward",
                table: "employee_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_subjects_tenant_id",
                schema: "DbReward",
                table: "subjects",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_tenant_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_semesters_tenant_id",
                schema: "DbReward",
                table: "subject_semesters",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_rewards_tenant_id",
                schema: "DbReward",
                table: "session_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_tenant_id",
                schema: "DbReward",
                table: "rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_rewards_tenant_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_rewards_tenant_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subjects_tenant_id",
                schema: "DbReward",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_subject_session_rewards_tenant_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_subject_semesters_tenant_id",
                schema: "DbReward",
                table: "subject_semesters");

            migrationBuilder.DropIndex(
                name: "IX_session_rewards_tenant_id",
                schema: "DbReward",
                table: "session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_rewards_tenant_id",
                schema: "DbReward",
                table: "rewards");

            migrationBuilder.DropIndex(
                name: "IX_employee_session_rewards_tenant_id",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_employee_rewards_tenant_id",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "subject_semesters");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "session_rewards");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "subjects",
                schema: "DbReward",
                newName: "subjects",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "subject_session_rewards",
                schema: "DbReward",
                newName: "subject_session_rewards",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "subject_semesters",
                schema: "DbReward",
                newName: "subject_semesters",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "session_rewards",
                schema: "DbReward",
                newName: "session_rewards",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "rewards",
                schema: "DbReward",
                newName: "rewards",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "employee_session_rewards",
                schema: "DbReward",
                newName: "employee_session_rewards",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "employee_rewards",
                schema: "DbReward",
                newName: "employee_rewards",
                newSchema: "dbo");
        }
    }
}
