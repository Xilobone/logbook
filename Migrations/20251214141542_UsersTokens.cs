using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class UsersTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Users",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "Environment",
                table: "Users",
                newName: "AccessToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Users",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "AccessToken",
                table: "Users",
                newName: "Environment");
        }
    }
}
