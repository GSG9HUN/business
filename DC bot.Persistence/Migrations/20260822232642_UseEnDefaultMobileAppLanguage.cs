using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseEnDefaultMobileAppLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "language_code",
                table: "mobile_app_user_settings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "hu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "language_code",
                table: "mobile_app_user_settings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "hu",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "en");
        }
    }
}
