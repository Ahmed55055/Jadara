using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardFlow_API.Migrations.RewardDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DbReward");

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "DbReward",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_theoretical = table.Column<bool>(type: "bit", nullable: false),
                    is_practical = table.Column<bool>(type: "bit", nullable: false),
                    subject_price = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

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
                name: "rewards",
                schema: "DbReward",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    total = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    last_update = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    reward_type = table.Column<int>(type: "int", nullable: false),
                    number_of_employees = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subject_semesters",
                schema: "DbReward",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    semester_number = table.Column<byte>(type: "tinyint", nullable: false),
                    number_of_students = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    year = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subject_semesters", x => x.id);
                    table.ForeignKey(
                        name: "FK_subject_semesters_courses_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "DbReward",
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_session_rewards",
                schema: "DbReward",
                columns: table => new
                {
                    session_reward_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    employee_snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sessions_count = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_session_rewards", x => new { x.session_reward_id, x.employee_id });
                    table.ForeignKey(
                        name: "FK_employee_session_rewards_employee_snapshots_employee_snapshot_id",
                        column: x => x.employee_snapshot_id,
                        principalSchema: "DbReward",
                        principalTable: "employee_snapshots",
                        principalColumn: "snapshot_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee_rewards",
                schema: "DbReward",
                columns: table => new
                {
                    reward_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    is_updated = table.Column<bool>(type: "bit", nullable: false),
                    RewardId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_rewards", x => new { x.reward_id, x.employee_id });
                    table.ForeignKey(
                        name: "FK_employee_rewards_employee_snapshots_snapshot_id",
                        column: x => x.snapshot_id,
                        principalSchema: "DbReward",
                        principalTable: "employee_snapshots",
                        principalColumn: "snapshot_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_rewards_rewards_RewardId1",
                        column: x => x.RewardId1,
                        principalSchema: "DbReward",
                        principalTable: "rewards",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_employee_rewards_rewards_reward_id",
                        column: x => x.reward_id,
                        principalSchema: "DbReward",
                        principalTable: "rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_rewards",
                schema: "DbReward",
                columns: table => new
                {
                    session_reward_id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<short>(type: "smallint", nullable: true),
                    semester = table.Column<byte>(type: "tinyint", nullable: true),
                    percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_rewards", x => x.session_reward_id);
                    table.CheckConstraint("CK_Session_Reward_Percentage_Min", "[percentage] >= 0");
                    table.ForeignKey(
                        name: "FK_session_rewards_rewards_session_reward_id",
                        column: x => x.session_reward_id,
                        principalSchema: "DbReward",
                        principalTable: "rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "subject_session_rewards",
                schema: "DbReward",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_reward_id = table.Column<int>(type: "int", nullable: false),
                    semester_subject_id = table.Column<int>(type: "int", nullable: false),
                    subject_snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    number_of_students = table.Column<int>(type: "int", nullable: false),
                    number_of_sessions = table.Column<int>(type: "int", nullable: false),
                    main_employee_id = table.Column<int>(type: "int", nullable: true),
                    max_number_of_employees = table.Column<short>(type: "smallint", nullable: false),
                    CourseSnapshotSnapshotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subject_session_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_subject_session_rewards_subject_semesters_semester_subject_id",
                        column: x => x.semester_subject_id,
                        principalSchema: "DbReward",
                        principalTable: "subject_semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subject_session_rewards_subject_snapshots_CourseSnapshotSnapshotId",
                        column: x => x.CourseSnapshotSnapshotId,
                        principalSchema: "DbReward",
                        principalTable: "subject_snapshots",
                        principalColumn: "snapshot_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subject_session_rewards_subject_snapshots_subject_snapshot_id",
                        column: x => x.subject_snapshot_id,
                        principalSchema: "DbReward",
                        principalTable: "subject_snapshots",
                        principalColumn: "snapshot_id");
                });

            migrationBuilder.CreateTable(
                name: "employee_session_subjects",
                schema: "DbReward",
                columns: table => new
                {
                    subject_session_reward_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    employee_snapshot_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseAssignmentId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_employee_session_subjects_subject_session_rewards_CourseAssignmentId",
                        column: x => x.CourseAssignmentId,
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
                name: "IX_courses_tenant_id",
                schema: "DbReward",
                table: "courses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_rewards_RewardId1",
                schema: "DbReward",
                table: "employee_rewards",
                column: "RewardId1");

            migrationBuilder.CreateIndex(
                name: "IX_employee_rewards_snapshot_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_rewards_tenant_id",
                schema: "DbReward",
                table: "employee_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_rewards_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_rewards",
                column: "employee_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_subjects_CourseAssignmentId",
                schema: "DbReward",
                table: "employee_session_subjects",
                column: "CourseAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_session_subjects_employee_snapshot_id",
                schema: "DbReward",
                table: "employee_session_subjects",
                column: "employee_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmployeeId_SnapshotDate",
                schema: "DbReward",
                table: "employee_snapshots",
                columns: new[] { "employee_id", "snapshot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_tenant_id",
                schema: "DbReward",
                table: "rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_rewards_tenant_id",
                schema: "DbReward",
                table: "session_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_semesters_subject_id",
                schema: "DbReward",
                table: "subject_semesters",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_semesters_tenant_id",
                schema: "DbReward",
                table: "subject_semesters",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_CourseSnapshotSnapshotId",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "CourseSnapshotSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_semester_subject_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "semester_subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_subject_snapshot_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "subject_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_session_rewards_tenant_id",
                schema: "DbReward",
                table: "subject_session_rewards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_subject_snapshots_semester_subject_id",
                schema: "DbReward",
                table: "subject_snapshots",
                column: "semester_subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_rewards",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "employee_session_rewards",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "employee_session_subjects",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "session_rewards",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "employee_snapshots",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "subject_session_rewards",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "rewards",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "subject_snapshots",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "subject_semesters",
                schema: "DbReward");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "DbReward");
        }
    }
}
