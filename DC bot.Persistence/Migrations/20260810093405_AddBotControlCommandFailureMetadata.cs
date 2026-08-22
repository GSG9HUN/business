using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBotControlCommandFailureMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "claimed_at_utc",
                table: "bot_control_commands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                table: "bot_control_commands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at_utc",
                table: "bot_control_commands",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "bot_control_commands",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bot_control_commands_status",
                table: "bot_control_commands",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bot_control_commands_status",
                table: "bot_control_commands");

            migrationBuilder.DropColumn(
                name: "claimed_at_utc",
                table: "bot_control_commands");

            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                table: "bot_control_commands");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                table: "bot_control_commands");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "bot_control_commands");
        }
    }
}
