using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class PersonalTemplateSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalEventTemplateSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventTemplateSetId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalEventTemplateSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalEventTemplateSet_EventTemplateSet_EventTemplateSetId",
                        column: x => x.EventTemplateSetId,
                        principalTable: "EventTemplateSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalEventTemplateSet_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalEventTemplateSet_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalEventTemplateSet_EventTemplateSetId",
                table: "PersonalEventTemplateSet",
                column: "EventTemplateSetId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalEventTemplateSet_GroupId",
                table: "PersonalEventTemplateSet",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalEventTemplateSet_UserId",
                table: "PersonalEventTemplateSet",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalEventTemplateSet");
        }
    }
}
