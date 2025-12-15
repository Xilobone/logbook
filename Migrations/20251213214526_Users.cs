using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Environment",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "UserName");
        }
    }
}
