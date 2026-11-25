using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class M2_SchedulingAndAccountManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_executed",
                table: "line_rich_menu_schedules");

            migrationBuilder.RenameColumn(
                name: "executed_at",
                table: "line_rich_menu_schedules",
                newName: "completed_at");

            migrationBuilder.AddColumn<string>(
                name: "line_rich_menu_id",
                table: "line_rich_menus",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "activated_at",
                table: "line_rich_menu_schedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "line_rich_menu_schedules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_default_line_rich_menu_id",
                table: "line_rich_menu_schedules",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "line_rich_menu_schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "action_uri",
                table: "line_rich_menu_areas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "action_data",
                table: "line_rich_menu_areas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "channel_secret",
                table: "line_official_accounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "channel_access_token",
                table: "line_official_accounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "basic_id",
                table: "line_official_accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_line_rich_menus_line_official_account_id_line_rich_menu_id",
                table: "line_rich_menus",
                columns: new[] { "line_official_account_id", "line_rich_menu_id" });

            migrationBuilder.CreateIndex(
                name: "IX_line_rich_menu_schedules_account_id",
                table: "line_rich_menu_schedules",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_line_rich_menu_schedules_status_start_time",
                table: "line_rich_menu_schedules",
                columns: new[] { "status", "start_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropIndex(
                name: "IX_line_rich_menus_line_official_account_id_line_rich_menu_id",
                table: "line_rich_menus");

            migrationBuilder.DropIndex(
                name: "IX_line_rich_menu_schedules_account_id",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropIndex(
                name: "IX_line_rich_menu_schedules_status_start_time",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "line_rich_menu_id",
                table: "line_rich_menus");

            migrationBuilder.DropColumn(
                name: "activated_at",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "previous_default_line_rich_menu_id",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "status",
                table: "line_rich_menu_schedules");

            migrationBuilder.DropColumn(
                name: "basic_id",
                table: "line_official_accounts");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "line_rich_menu_schedules",
                newName: "executed_at");

            migrationBuilder.AddColumn<bool>(
                name: "is_executed",
                table: "line_rich_menu_schedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "action_uri",
                table: "line_rich_menu_areas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "action_data",
                table: "line_rich_menu_areas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "channel_secret",
                table: "line_official_accounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "channel_access_token",
                table: "line_official_accounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
