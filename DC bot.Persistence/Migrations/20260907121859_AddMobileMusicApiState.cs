using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileMusicApiState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_paused",
                table: "guild_playback_state",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "position_seconds",
                table: "guild_playback_state",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "position_updated_at_utc",
                table: "guild_playback_state",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payload_json",
                table: "bot_control_commands",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_paused",
                table: "guild_playback_state");

            migrationBuilder.DropColumn(
                name: "position_seconds",
                table: "guild_playback_state");

            migrationBuilder.DropColumn(
                name: "position_updated_at_utc",
                table: "guild_playback_state");

            migrationBuilder.DropColumn(
                name: "payload_json",
                table: "bot_control_commands");
        }
    }
}
