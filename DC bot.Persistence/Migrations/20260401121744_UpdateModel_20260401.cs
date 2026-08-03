using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel_20260401 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildQueueItems_guild_data_GuildId",
                table: "GuildQueueItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildQueueItems",
                table: "GuildQueueItems");

            migrationBuilder.DropIndex(
                name: "IX_GuildQueueItems_GuildId",
                table: "GuildQueueItems");

            migrationBuilder.RenameTable(
                name: "GuildQueueItems",
                newName: "guild_queue_item");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "guild_queue_item",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "guild_queue_item",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guild_queue_item",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TrackIdentifier",
                table: "guild_queue_item",
                newName: "track_identifier");

            migrationBuilder.RenameColumn(
                name: "SkippedAtUtc",
                table: "guild_queue_item",
                newName: "skipped_at_utc");

            migrationBuilder.RenameColumn(
                name: "PlayedAtUtc",
                table: "guild_queue_item",
                newName: "played_at_utc");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "guild_queue_item",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "AddedAtUtc",
                table: "guild_queue_item",
                newName: "added_at_utc");

            migrationBuilder.AlterColumn<short>(
                name: "state",
                table: "guild_queue_item",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "added_at_utc",
                table: "guild_queue_item",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_queue_item",
                table: "guild_queue_item",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_state_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "state", "position" });

            migrationBuilder.AddForeignKey(
                name: "FK_guild_queue_item_guild_data_guild_id",
                table: "guild_queue_item",
                column: "guild_id",
                principalTable: "guild_data",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_queue_item_guild_data_guild_id",
                table: "guild_queue_item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_queue_item",
                table: "guild_queue_item");

            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item");

            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_state_position",
                table: "guild_queue_item");

            migrationBuilder.RenameTable(
                name: "guild_queue_item",
                newName: "GuildQueueItems");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "GuildQueueItems",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "GuildQueueItems",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "GuildQueueItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "track_identifier",
                table: "GuildQueueItems",
                newName: "TrackIdentifier");

            migrationBuilder.RenameColumn(
                name: "skipped_at_utc",
                table: "GuildQueueItems",
                newName: "SkippedAtUtc");

            migrationBuilder.RenameColumn(
                name: "played_at_utc",
                table: "GuildQueueItems",
                newName: "PlayedAtUtc");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "GuildQueueItems",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "added_at_utc",
                table: "GuildQueueItems",
                newName: "AddedAtUtc");

            migrationBuilder.AlterColumn<short>(
                name: "State",
                table: "GuildQueueItems",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "AddedAtUtc",
                table: "GuildQueueItems",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildQueueItems",
                table: "GuildQueueItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildQueueItems_GuildId",
                table: "GuildQueueItems",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildQueueItems_guild_data_GuildId",
                table: "GuildQueueItems",
                column: "GuildId",
                principalTable: "guild_data",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
