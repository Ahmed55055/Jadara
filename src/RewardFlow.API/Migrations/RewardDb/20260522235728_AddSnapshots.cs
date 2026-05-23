using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.RewardDb
{
    /// <inheritdoc />
    public partial class AddSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_rewards_rewards_RewardEntityId",
                schema: "dbo",
                table: "employee_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_session_rewards_subject_session_rewards_subject_session_reward_id",
                schema: "dbo",
                table: "employee_session_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_subject_session_rewards_subject_semesters_subject_id",
                schema: "dbo",
                table: "subject_session_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_session_rewards",
                schema: "dbo",
                table: "employee_session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_employee_session_rewards_subject_session_reward_id",
                schema: "dbo",
                table: "employee_session_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_rewards",
                schema: "dbo",
                table: "employee_rewards");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "dbo",
                table: "employee_session_rewards");

            migrationBuilder.DropColumn(
                name: "Salary",
                schema: "dbo",
                table: "employee_session_rewards");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "dbo",
                table: "employee_rewards");

            migrationBuilder.DropColumn(
                name: "total",
                schema: "dbo",
                table: "employee_rewards");

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

            migrationBuilder.RenameColumn(
                name: "subject_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                newName: "semester_subject_id");

            migrationBuilder.RenameColumn(
                name: "students_number",
                schema: "DbReward",
                table: "subject_session_rewards",
                newName: "number_of_students");

            migrationBuilder.RenameIndex(
                name: "IX_subject_session_rewards_subject_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                newName: "IX_subject_session_rewards_semester_subject_id");

            migrationBuilder.RenameColumn(
                name: "Year",
                schema: "DbReward",
                table: "session_rewards",
                newName: "year");

            migrationBuilder.RenameColumn(
                name: "Semester",
                schema: "DbReward",
                table: "session_rewards",
                newName: "semester");

            migrationBuilder.RenameColumn(
                name: "subject_session_reward_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                newName: "sessions_count");

            migrationBuilder.RenameColumn(
                name: "NumberOfSessions",
                schema: "DbReward",
                table: "employee_session_rewards",
                newName: "session_reward_id");

            migrationBuilder.RenameColumn(
                name: "IsUpdated",
                schema: "DbReward",
                table: "employee_rewards",
                newName: "is_updated");

            migrationBuilder.RenameColumn(
                name: "RewardEntityId",
                schema: "DbReward",
                table: "employee_rewards",
                newName: "RewardId1");

            migrationBuilder.RenameIndex(
                name: "IX_employee_rewards_RewardEntityId",
                schema: "DbReward",
                table: "employee_rewards",
                newName: "IX_employee_rewards_RewardId1");

            migrationBuilder.AlterColumn<decimal>(
                name: "subject_price",
                schema: "DbReward",
                table: "subjects",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "smallmoney");

            migrationBuilder.AlterColumn<short>(
                name: "max_number_of_employees",
                schema: "DbReward",
                table: "subject_session_rewards",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "DbReward",
                table: "subject_semesters",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "year",
                schema: "DbReward",
                table: "subject_semesters",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<decimal>(
                name: "percentage",
                schema: "DbReward",
                table: "session_rewards",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<short>(
                name: "year",
                schema: "DbReward",
                table: "session_rewards",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "total",
                schema: "DbReward",
                table: "rewards",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "smallmoney");

            migrationBuilder.AddColumn<Guid>(
                name: "employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "DbReward",
                table: "employee_session_rewards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                schema: "DbReward",
                table: "employee_rewards",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "snapshot_id",
                schema: "DbReward",
                table: "employee_rewards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_session_rewards",
                schema: "DbReward",
                table: "employee_session_rewards",
                columns: new[] { "session_reward_id", "employee_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_rewards",
                schema: "DbReward",
                table: "employee_rewards",
                columns: new[] { "reward_id", "employee_id" });

            migrationBuilder.CreateTable(
                name: "employee_snapshots",
                schema: "DbReward",
                columns: table => new
                {
                    snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    snapshot_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    national_number = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    account_number = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    national_number_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    account_number_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    salary = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    job_title = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_snapshots", x => x.snapshot_id);
                });

            migrationBuilder.CreateTable(
                name: "subject_snapshots",
                schema: "DbReward",
                columns: table => new
                {
                    snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    captured_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    semester_subject_id = table.Column<int>(type: "int", nullable: false),
                    subject_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_theoretical = table.Column<bool>(type: "bit", nullable: false),
                    is_practical = table.Column<bool>(type: "bit", nullable: false),
                    semester = table.Column<byte>(type: "tinyint", nullable: false),
                    year = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subject_snapshots", x => x.snapshot_id);
                    table.ForeignKey(
                        name: "FK_subject_snapshots_subject_semesters_semester_subject_id",
                        column: x => x.semester_subject_id,
                        principalSchema: "DbReward",
                        principalTable: "subject_semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee_session_subjects",
                schema: "DbReward",
                columns: table => new
                {
                    subject_session_reward_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    employee_snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectSessionRewardEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_session_subjects", x => new { x.subject_session_reward_id, x.employee_id });
                    table.ForeignKey(
                        name: "FK_employee_session_subjects_employee_snapshots_employee_snapshot_id",
                        column: x => x.employee_snapshot_id,
                        principalSchema: "DbReward",
                        principalTable: "employee_snapshots",
                        principalColumn: "snapshot_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_session_subjects_subject_session_rewards_SubjectSessionRewardEntityId",
                        column: x => x.SubjectSessionRewardEntityId,
                        principalSchema: "DbReward",
                        principalTable: "subject_session_rewards",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_employee_session_subjects_subject_session_rewards_subject_session_reward_id",
                        column: x => x.subject_session_reward_id,
                        principalSchema: "DbReward",
                        principalTable: "subject_session_rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "subject_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "SubjectSnapshotSnapshotId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Session_Reward_Percentage_Min",
                schema: "DbReward",
                table: "session_rewards",
                sql: "[percentage] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_rewards_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                column: "employee_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_rewards_snapshot_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_subjects_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_subjects",
                column: "employee_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_subjects_SubjectSessionRewardEntityId",
                schema: "DbReward",
                table: "employee_session_subjects",
                column: "SubjectSessionRewardEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmployeeId_SnapshotDate",
                schema: "DbReward",
                table: "employee_snapshots",
                columns: new[] { "employee_id", "snapshot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_subject_snapshots_semester_subject_id",
                schema: "DbReward",
                table: "subject_snapshots",
                column: "semester_subject_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_rewards_employee_snapshots_snapshot_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "snapshot_id",
                principalSchema: "DbReward",
                principalTable: "employee_snapshots",
                principalColumn: "snapshot_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_rewards_rewards_RewardId1",
                schema: "DbReward",
                table: "employee_rewards",
                column: "RewardId1",
                principalSchema: "DbReward",
                principalTable: "rewards",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_rewards_rewards_reward_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "reward_id",
                principalSchema: "DbReward",
                principalTable: "rewards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_session_rewards_employee_snapshots_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                column: "employee_snapshot_id",
                principalSchema: "DbReward",
                principalTable: "employee_snapshots",
                principalColumn: "snapshot_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_subject_session_rewards_subject_semesters_semester_subject_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "semester_subject_id",
                principalSchema: "DbReward",
                principalTable: "subject_semesters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subject_session_rewards_subject_snapshots_SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "SubjectSnapshotSnapshotId",
                principalSchema: "DbReward",
                principalTable: "subject_snapshots",
                principalColumn: "snapshot_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subject_session_rewards_subject_snapshots_subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "subject_snapshot_id",
                principalSchema: "DbReward",
                principalTable: "subject_snapshots",
                principalColumn: "snapshot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_rewards_employee_snapshots_snapshot_id",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_rewards_rewards_RewardId1",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_rewards_rewards_reward_id",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_session_rewards_employee_snapshots_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_subject_session_rewards_subject_semesters_semester_subject_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_subject_session_rewards_subject_snapshots_SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_subject_session_rewards_subject_snapshots_subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropTable(
                name: "employee_session_subjects",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "subject_snapshots",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "employee_snapshots",
                schema: "DbReward");

            migrationBuilder.DropIndex(
                name: "IX_subject_session_rewards_subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_subject_session_rewards_SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Session_Reward_Percentage_Min",
                schema: "DbReward",
                table: "session_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_session_rewards",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropIndex(
                name: "IX_employee_session_rewards_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_rewards",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropIndex(
                name: "IX_employee_rewards_snapshot_id",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropColumn(
                name: "SubjectSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropColumn(
                name: "subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards");

            migrationBuilder.DropColumn(
                name: "year",
                schema: "DbReward",
                table: "subject_semesters");

            migrationBuilder.DropColumn(
                name: "employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "DbReward",
                table: "employee_session_rewards");

            migrationBuilder.DropColumn(
                name: "amount",
                schema: "DbReward",
                table: "employee_rewards");

            migrationBuilder.DropColumn(
                name: "snapshot_id",
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

            migrationBuilder.RenameColumn(
                name: "semester_subject_id",
                schema: "dbo",
                table: "subject_session_rewards",
                newName: "subject_id");

            migrationBuilder.RenameColumn(
                name: "number_of_students",
                schema: "dbo",
                table: "subject_session_rewards",
                newName: "students_number");

            migrationBuilder.RenameIndex(
                name: "IX_subject_session_rewards_semester_subject_id",
                schema: "dbo",
                table: "subject_session_rewards",
                newName: "IX_subject_session_rewards_subject_id");

            migrationBuilder.RenameColumn(
                name: "year",
                schema: "dbo",
                table: "session_rewards",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "semester",
                schema: "dbo",
                table: "session_rewards",
                newName: "Semester");

            migrationBuilder.RenameColumn(
                name: "sessions_count",
                schema: "dbo",
                table: "employee_session_rewards",
                newName: "subject_session_reward_id");

            migrationBuilder.RenameColumn(
                name: "session_reward_id",
                schema: "dbo",
                table: "employee_session_rewards",
                newName: "NumberOfSessions");

            migrationBuilder.RenameColumn(
                name: "is_updated",
                schema: "dbo",
                table: "employee_rewards",
                newName: "IsUpdated");

            migrationBuilder.RenameColumn(
                name: "RewardId1",
                schema: "dbo",
                table: "employee_rewards",
                newName: "RewardEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_rewards_RewardId1",
                schema: "dbo",
                table: "employee_rewards",
                newName: "IX_employee_rewards_RewardEntityId");

            migrationBuilder.AlterColumn<decimal>(
                name: "subject_price",
                schema: "dbo",
                table: "subjects",
                type: "smallmoney",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AlterColumn<int>(
                name: "max_number_of_employees",
                schema: "dbo",
                table: "subject_session_rewards",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Price",
                schema: "dbo",
                table: "subject_semesters",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                schema: "dbo",
                table: "session_rewards",
                type: "int",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "percentage",
                schema: "dbo",
                table: "session_rewards",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "total",
                schema: "dbo",
                table: "rewards",
                type: "smallmoney",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "dbo",
                table: "employee_session_rewards",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<float>(
                name: "Salary",
                schema: "dbo",
                table: "employee_session_rewards",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "dbo",
                table: "employee_rewards",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<decimal>(
                name: "total",
                schema: "dbo",
                table: "employee_rewards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_session_rewards",
                schema: "dbo",
                table: "employee_session_rewards",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_rewards",
                schema: "dbo",
                table: "employee_rewards",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_rewards_subject_session_reward_id",
                schema: "dbo",
                table: "employee_session_rewards",
                column: "subject_session_reward_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_rewards_rewards_RewardEntityId",
                schema: "dbo",
                table: "employee_rewards",
                column: "RewardEntityId",
                principalSchema: "dbo",
                principalTable: "rewards",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_session_rewards_subject_session_rewards_subject_session_reward_id",
                schema: "dbo",
                table: "employee_session_rewards",
                column: "subject_session_reward_id",
                principalSchema: "dbo",
                principalTable: "subject_session_rewards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subject_session_rewards_subject_semesters_subject_id",
                schema: "dbo",
                table: "subject_session_rewards",
                column: "subject_id",
                principalSchema: "dbo",
                principalTable: "subject_semesters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
