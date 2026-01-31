using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class EventTitleAndBody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventPrefix",
                table: "Groups",
                newName: "EventTitle");

            migrationBuilder.AddColumn<string>(
                name: "EventBody",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventBody",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "EventTitle",
                table: "Groups",
                newName: "EventPrefix");
        }
    }
}
