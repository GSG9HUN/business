using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybackBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "batch_id",
                table: "guild_queue_item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "batch_item_id",
                table: "guild_queue_item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "active_batch_id",
                table: "guild_playback_state",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "playback_batches",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playback_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_playback_batches_guild_data_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playback_batch_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<long>(type: "bigint", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    track_identifier = table.Column<string>(type: "text", nullable: false),
                    source_query = table.Column<string>(type: "text", nullable: true),
                    source_search_mode = table.Column<string>(type: "text", nullable: true),
                    requested_by = table.Column<string>(type: "text", nullable: true),
                    added_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playback_batch_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_playback_batch_items_playback_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "playback_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_batch_id_state_position",
                table: "guild_queue_item",
                columns: new[] { "batch_id", "state", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_batch_item_id",
                table: "guild_queue_item",
                column: "batch_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_playback_state_active_batch_id",
                table: "guild_playback_state",
                column: "active_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_playback_batch_items_batch_id_position",
                table: "playback_batch_items",
                columns: new[] { "batch_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_playback_batches_guild_id",
                table: "playback_batches",
                column: "guild_id",
                unique: true,
                filter: "completed_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_playback_batches_guild_id_created_at_utc",
                table: "playback_batches",
                columns: new[] { "guild_id", "created_at_utc" });

            migrationBuilder.AddForeignKey(
                name: "FK_guild_playback_state_playback_batches_active_batch_id",
                table: "guild_playback_state",
                column: "active_batch_id",
                principalTable: "playback_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_queue_item_playback_batch_items_batch_item_id",
                table: "guild_queue_item",
                column: "batch_item_id",
                principalTable: "playback_batch_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_queue_item_playback_batches_batch_id",
                table: "guild_queue_item",
                column: "batch_id",
                principalTable: "playback_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_playback_state_playback_batches_active_batch_id",
                table: "guild_playback_state");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_queue_item_playback_batch_items_batch_item_id",
                table: "guild_queue_item");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_queue_item_playback_batches_batch_id",
                table: "guild_queue_item");

            migrationBuilder.DropTable(
                name: "playback_batch_items");

            migrationBuilder.DropTable(
                name: "playback_batches");

            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_batch_id_state_position",
                table: "guild_queue_item");

            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_batch_item_id",
                table: "guild_queue_item");

            migrationBuilder.DropIndex(
                name: "IX_guild_playback_state_active_batch_id",
                table: "guild_playback_state");

            migrationBuilder.DropColumn(
                name: "batch_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "batch_item_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "active_batch_id",
                table: "guild_playback_state");
        }
    }
}
