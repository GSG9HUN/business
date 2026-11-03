using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "enqueue_group_id",
                table: "playback_batch_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_removed",
                table: "playback_batch_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "batch_position",
                table: "guild_queue_item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "enqueue_group_id",
                table: "guild_queue_item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_enqueue_group_id",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "enqueue_group_id" });

            // Existing canonical ownership is preserved; old enqueue boundaries cannot be recovered.
            migrationBuilder.Sql("""
                UPDATE playback_batch_items
                SET enqueue_group_id = md5('legacy-canonical-' || id::text)::uuid;
                UPDATE guild_queue_item
                SET enqueue_group_id = md5('legacy-queue-' || id::text)::uuid,
                    batch_position = position;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_enqueue_group_id",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "enqueue_group_id",
                table: "playback_batch_items");

            migrationBuilder.DropColumn(
                name: "is_removed",
                table: "playback_batch_items");

            migrationBuilder.DropColumn(
                name: "batch_position",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "enqueue_group_id",
                table: "guild_queue_item");
        }
    }
}
