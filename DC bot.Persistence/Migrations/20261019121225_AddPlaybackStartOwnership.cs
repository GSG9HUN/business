using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybackStartOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item");

            migrationBuilder.AddColumn<Guid>(
                name: "playback_operation_id",
                table: "guild_queue_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "start_previous_operation_id",
                table: "guild_queue_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "start_previous_state",
                table: "guild_queue_item",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "position" },
                unique: true,
                filter: "\"state\" IN (0, 4)");

            migrationBuilder.CreateIndex(
                name: "ux_queue_starting_guild",
                table: "guild_queue_item",
                column: "guild_id",
                unique: true,
                filter: "\"state\" = 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item");

            migrationBuilder.DropIndex(
                name: "ux_queue_starting_guild",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "playback_operation_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "start_previous_operation_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "start_previous_state",
                table: "guild_queue_item");

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "position" },
                unique: true,
                filter: "\"state\" = 0");
        }
    }
}
