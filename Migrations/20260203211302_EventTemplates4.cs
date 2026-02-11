using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class EventTemplates4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendingStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeclinedStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TentativeStatus",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "ShowAs",
                table: "EventTemplate",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowAs",
                table: "EventTemplate");

            migrationBuilder.AddColumn<int>(
                name: "AttendingStatus",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeclinedStatus",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TentativeStatus",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
