using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueItemSourceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_query",
                table: "guild_queue_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_search_mode",
                table: "guild_queue_item",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_query",
                table: "guild_queue_item");

            migrationBuilder.DropColumn(
                name: "source_search_mode",
                table: "guild_queue_item");
        }
    }
}
