using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class EventTemplates3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet",
                column: "GroupId",
                unique: true);
        }
    }
}
