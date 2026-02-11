using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class EventTemplates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventTitle = table.Column<string>(type: "TEXT", nullable: false),
                    EventBody = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTemplateSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DifferentiateOnAttendance = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttendingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TentativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnavailableId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplateSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTemplateSet_EventTemplate_AttendingId",
                        column: x => x.AttendingId,
                        principalTable: "EventTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTemplateSet_EventTemplate_TentativeId",
                        column: x => x.TentativeId,
                        principalTable: "EventTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTemplateSet_EventTemplate_UnavailableId",
                        column: x => x.UnavailableId,
                        principalTable: "EventTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTemplateSet_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTemplateSet_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_AttendingId",
                table: "EventTemplateSet",
                column: "AttendingId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_TentativeId",
                table: "EventTemplateSet",
                column: "TentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_UnavailableId",
                table: "EventTemplateSet",
                column: "UnavailableId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_UserId",
                table: "EventTemplateSet",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTemplateSet");

            migrationBuilder.DropTable(
                name: "EventTemplate");
        }
    }
}
