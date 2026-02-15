using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace logbook.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedEventTemplateSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTemplateSet_Groups_GroupId",
                table: "EventTemplateSet");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTemplateSet_Users_UserId",
                table: "EventTemplateSet");

            migrationBuilder.DropIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet");

            migrationBuilder.DropIndex(
                name: "IX_EventTemplateSet_UserId",
                table: "EventTemplateSet");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "EventTemplateSet");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EventTemplateSet");

            migrationBuilder.RenameColumn(
                name: "EventTitle",
                table: "EventTemplate",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "EventBody",
                table: "EventTemplate",
                newName: "Body");

            migrationBuilder.AddColumn<Guid>(
                name: "EventTemplateSetId",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Groups_EventTemplateSetId",
                table: "Groups",
                column: "EventTemplateSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_EventTemplateSet_EventTemplateSetId",
                table: "Groups",
                column: "EventTemplateSetId",
                principalTable: "EventTemplateSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_EventTemplateSet_EventTemplateSetId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_EventTemplateSetId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "EventTemplateSetId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "EventTemplate",
                newName: "EventTitle");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "EventTemplate",
                newName: "EventBody");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "EventTemplateSet",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "EventTemplateSet",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_GroupId",
                table: "EventTemplateSet",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateSet_UserId",
                table: "EventTemplateSet",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTemplateSet_Groups_GroupId",
                table: "EventTemplateSet",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTemplateSet_Users_UserId",
                table: "EventTemplateSet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
