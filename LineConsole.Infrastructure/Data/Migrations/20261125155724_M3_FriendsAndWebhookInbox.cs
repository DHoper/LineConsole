using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LineConsole.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class M3_FriendsAndWebhookInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "line_friends",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    account_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    line_user_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    picture_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    followed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    blocked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_status_event_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line_friends", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "line_webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    account_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    webhook_event_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    line_user_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    occurred_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_redelivery = table.Column<bool>(type: "bit", nullable: false),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line_webhook_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_line_friends_account_id_line_user_id",
                table: "line_friends",
                columns: new[] { "account_id", "line_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_line_webhook_events_account_id_occurred_at",
                table: "line_webhook_events",
                columns: new[] { "account_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_line_webhook_events_account_id_webhook_event_id",
                table: "line_webhook_events",
                columns: new[] { "account_id", "webhook_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_line_webhook_events_status_occurred_at",
                table: "line_webhook_events",
                columns: new[] { "status", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "line_friends");

            migrationBuilder.DropTable(
                name: "line_webhook_events");
        }
    }
}
