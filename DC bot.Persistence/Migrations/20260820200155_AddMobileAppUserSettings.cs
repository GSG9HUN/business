using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileAppUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_app_user_settings",
                columns: table => new
                {
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "hu"),
                    theme = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "dark"),
                    haptic_feedback_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sound_effects_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    telemetry_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_app_user_settings", x => x.discord_user_id);
                    table.ForeignKey(
                        name: "FK_mobile_app_user_settings_mobile_app_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalTable: "mobile_app_users",
                        principalColumn: "discord_user_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_app_user_settings");
        }
    }
}
