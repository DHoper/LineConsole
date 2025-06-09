using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class revise_Richmenu_rel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_line_rich_menu_schedules_line_rich_menus_RichMenuEntityId",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropIndex(
                name: "IX_line_rich_menu_schedules_RichMenuEntityId",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "RichMenuEntityId",
                table: "line_rich_menu_schedules");

            migrationBuilder.CreateIndex(
                name: "IX_line_rich_menu_schedules_rich_menu_id",
                table: "line_rich_menu_schedules",
                column: "rich_menu_id");

            migrationBuilder.AddForeignKey(
                name: "FK_line_rich_menu_schedules_line_rich_menus_rich_menu_id",
                table: "line_rich_menu_schedules",
                column: "rich_menu_id",
                principalTable: "line_rich_menus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_line_rich_menu_schedules_line_rich_menus_rich_menu_id",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropIndex(
                name: "IX_line_rich_menu_schedules_rich_menu_id",
                table: "line_rich_menu_schedules");

            migrationBuilder.AddColumn<Guid>(
                name: "RichMenuEntityId",
                table: "line_rich_menu_schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_line_rich_menu_schedules_RichMenuEntityId",
                table: "line_rich_menu_schedules",
                column: "RichMenuEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_line_rich_menu_schedules_line_rich_menus_RichMenuEntityId",
                table: "line_rich_menu_schedules",
                column: "RichMenuEntityId",
                principalTable: "line_rich_menus",
                principalColumn: "id");
        }
    }
}
