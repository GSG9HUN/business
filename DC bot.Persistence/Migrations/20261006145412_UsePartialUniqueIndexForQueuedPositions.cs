using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class UsePartialUniqueIndexForQueuedPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item");

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "position" },
                unique: true,
                filter: "\"state\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item");

            migrationBuilder.CreateIndex(
                name: "IX_guild_queue_item_guild_id_position",
                table: "guild_queue_item",
                columns: new[] { "guild_id", "position" },
                unique: true);
        }
    }
}
