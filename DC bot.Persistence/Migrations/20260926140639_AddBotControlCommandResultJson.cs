using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddBotControlCommandResultJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "result_json",
                table: "bot_control_commands",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "result_json",
                table: "bot_control_commands");
        }
    }
}
