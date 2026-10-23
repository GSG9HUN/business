using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddTwentyFourSevenSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_twenty_four_seven_enabled",
                table: "guild_data",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "preferred_text_channel_id",
                table: "guild_data",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "preferred_voice_channel_id",
                table: "guild_data",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_twenty_four_seven_enabled",
                table: "guild_data");

            migrationBuilder.DropColumn(
                name: "preferred_text_channel_id",
                table: "guild_data");

            migrationBuilder.DropColumn(
                name: "preferred_voice_channel_id",
                table: "guild_data");
        }
    }
}
