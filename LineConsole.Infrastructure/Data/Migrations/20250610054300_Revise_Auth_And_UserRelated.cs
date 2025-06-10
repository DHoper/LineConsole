using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Revise_Auth_And_UserRelated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_line_official_accounts_user_profile_id",
                table: "line_official_accounts",
                column: "user_profile_id");

            migrationBuilder.AddForeignKey(
                name: "FK_line_official_accounts_user_profiles_user_profile_id",
                table: "line_official_accounts",
                column: "user_profile_id",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_line_official_accounts_user_profiles_user_profile_id",
                table: "line_official_accounts");

            migrationBuilder.DropIndex(
                name: "IX_line_official_accounts_user_profile_id",
                table: "line_official_accounts");
        }
    }
}
