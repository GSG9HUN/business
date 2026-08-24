using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseSystemDefaultMobileAppTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "theme",
                table: "mobile_app_user_settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "system",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "dark");

            migrationBuilder.AddCheckConstraint(
                name: "ck_mobile_app_user_settings_theme",
                table: "mobile_app_user_settings",
                sql: "theme IN ('dark', 'light', 'system')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_mobile_app_user_settings_theme",
                table: "mobile_app_user_settings");

            migrationBuilder.AlterColumn<string>(
                name: "theme",
                table: "mobile_app_user_settings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "dark",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "system");
        }
    }
}
