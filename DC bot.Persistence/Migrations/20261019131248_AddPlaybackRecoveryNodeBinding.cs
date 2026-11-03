using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybackRecoveryNodeBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_runtime_status");

            migrationBuilder.AddColumn<string>(
                name: "playback_node_address",
                table: "guild_queue_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "playback_node_session_id",
                table: "guild_queue_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "playback_transport_confirmed",
                table: "guild_queue_item",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "playback_node_address",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "playback_node_session_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "playback_transport_confirmed",
                table: "guild_queue_item");

            migrationBuilder.CreateTable(
                name: "bot_runtime_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    last_heartbeat_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_runtime_status", x => x.id);
                });
        }
    }
}
