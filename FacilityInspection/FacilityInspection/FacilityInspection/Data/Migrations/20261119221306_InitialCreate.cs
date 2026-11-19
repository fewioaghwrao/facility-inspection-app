using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FacilityInspection.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FactorySites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactorySites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EquipmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoginId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NormalizedLoginId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FactorySiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Floor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_FactorySites_FactorySiteId",
                        column: x => x.FactorySiteId,
                        principalTable: "FactorySites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MinimumValue = table.Column<double>(type: "REAL", nullable: true),
                    MaximumValue = table.Column<double>(type: "REAL", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTemplateItems_InspectionTemplates_InspectionTemplateId",
                        column: x => x.InspectionTemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OperatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityType = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BeforeValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    AfterValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EquipmentType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ModelNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InstalledOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipments_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedOperatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSchedules_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionSchedules_InspectionTemplates_InspectionTemplateId",
                        column: x => x.InspectionTemplateId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionSchedules_Operators_AssignedOperatorId",
                        column: x => x.AssignedOperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionScheduleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PerformedByOperatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReturnReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_InspectionSchedules_InspectionScheduleId",
                        column: x => x.InspectionScheduleId,
                        principalTable: "InspectionSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inspections_Operators_PerformedByOperatorId",
                        column: x => x.PerformedByOperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionTemplateItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    NumericValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    TextValue = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionResults_InspectionTemplateItems_InspectionTemplateItemId",
                        column: x => x.InspectionTemplateItemId,
                        principalTable: "InspectionTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionResults_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionResultId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionPhotos_InspectionResults_InspectionResultId",
                        column: x => x.InspectionResultId,
                        principalTable: "InspectionResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionPhotos_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FactorySites",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "Description", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "SITE-001", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "第1工場の設備を管理する工場マスター", true, "第1工場", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "SITE-002", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "第2工場の設備を管理する工場マスター", true, "第2工場", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "Description", "FactorySiteId", "Floor", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "COMPRESSOR-ROOM", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "エアコンプレッサーを設置する区域", new Guid("10000000-0000-0000-0000-000000000001"), "1F", true, "コンプレッサー室", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "PUMP-ROOM", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "冷却水ポンプを設置する区域", new Guid("10000000-0000-0000-0000-000000000001"), "1F", true, "ポンプ室", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "VENTILATION-ROOM", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "換気設備を設置する区域", new Guid("10000000-0000-0000-0000-000000000001"), "2F", true, "換気設備室", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "PUMP-ROOM", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "冷却水ポンプを設置する区域", new Guid("10000000-0000-0000-0000-000000000002"), "1F", true, "ポンプ室", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "DUST-COLLECTION-ROOM", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "集塵設備を設置する区域", new Guid("10000000-0000-0000-0000-000000000002"), "2F", true, "集塵設備室", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "OUTDOOR-AREA", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "屋外設備を設置する区域", new Guid("10000000-0000-0000-0000-000000000002"), null, true, "屋外設備エリア", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionType",
                table: "AuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUtc",
                table: "AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OperatorId",
                table: "AuditLogs",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_EquipmentCode",
                table: "Equipments",
                column: "EquipmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_EquipmentType",
                table: "Equipments",
                column: "EquipmentType");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_LocationId",
                table: "Equipments",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Status",
                table: "Equipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FactorySites_Code",
                table: "FactorySites",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPhotos_InspectionId_DisplayOrder",
                table: "InspectionPhotos",
                columns: new[] { "InspectionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPhotos_InspectionResultId",
                table: "InspectionPhotos",
                column: "InspectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_InspectionId_DisplayOrder",
                table: "InspectionResults",
                columns: new[] { "InspectionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_InspectionId_InspectionTemplateItemId",
                table: "InspectionResults",
                columns: new[] { "InspectionId", "InspectionTemplateItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResults_InspectionTemplateItemId",
                table: "InspectionResults",
                column: "InspectionTemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectionScheduleId",
                table: "Inspections",
                column: "InspectionScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_PerformedByOperatorId",
                table: "Inspections",
                column: "PerformedByOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_AssignedOperatorId",
                table: "InspectionSchedules",
                column: "AssignedOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_EquipmentId_ScheduledDate",
                table: "InspectionSchedules",
                columns: new[] { "EquipmentId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_InspectionTemplateId",
                table: "InspectionSchedules",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_ScheduledDate",
                table: "InspectionSchedules",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplateItems_InspectionTemplateId_DisplayOrder",
                table: "InspectionTemplateItems",
                columns: new[] { "InspectionTemplateId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplates_EquipmentType_Version",
                table: "InspectionTemplates",
                columns: new[] { "EquipmentType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_FactorySiteId_Code",
                table: "Locations",
                columns: new[] { "FactorySiteId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operators_NormalizedLoginId",
                table: "Operators",
                column: "NormalizedLoginId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "InspectionPhotos");

            migrationBuilder.DropTable(
                name: "InspectionResults");

            migrationBuilder.DropTable(
                name: "InspectionTemplateItems");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "InspectionSchedules");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "InspectionTemplates");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "FactorySites");
        }
    }
}
