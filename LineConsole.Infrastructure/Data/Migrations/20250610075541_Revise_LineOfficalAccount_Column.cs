using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Revise_LineOfficalAccount_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "line_user_id",
                table: "line_official_accounts",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "access_token",
                table: "line_official_accounts",
                newName: "channel_secret");

            migrationBuilder.AddColumn<string>(
                name: "channel_access_token",
                table: "line_official_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel_access_token",
                table: "line_official_accounts");

            migrationBuilder.RenameColumn(
                name: "channel_secret",
                table: "line_official_accounts",
                newName: "access_token");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "line_official_accounts",
                newName: "line_user_id");
        }
    }
}
