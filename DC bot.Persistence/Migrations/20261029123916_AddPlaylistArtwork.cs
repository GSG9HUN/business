using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistArtwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "artwork_uri",
                table: "playlists",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "artwork_uri",
                table: "playlists");
        }
    }
}
