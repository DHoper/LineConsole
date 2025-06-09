using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitIdentitySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "user_profiles");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "line_rich_menus",
                newName: "line_official_account_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "line_official_accounts",
                newName: "user_profile_id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "user_profiles",
                newName: "organization_code");

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "user_profiles",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "user_profiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identity_user_id",
                table: "user_profiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_profiles",
                table: "user_profiles",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_profiles",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "identity_user_id",
                table: "user_profiles");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                newName: "users");

            migrationBuilder.RenameColumn(
                name: "line_official_account_id",
                table: "line_rich_menus",
                newName: "account_id");

            migrationBuilder.RenameColumn(
                name: "user_profile_id",
                table: "line_official_accounts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "organization_code",
                table: "users",
                newName: "name");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");
        }
    }
}
