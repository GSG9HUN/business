using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileAppSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_app_sessions",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRefreshedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshTokenExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_app_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_mobile_app_sessions_mobile_app_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalTable: "mobile_app_users",
                        principalColumn: "discord_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_app_sessions_discord_user_id",
                table: "mobile_app_sessions",
                column: "discord_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_app_sessions_refresh_token_hash",
                table: "mobile_app_sessions",
                column: "refresh_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_app_sessions");
        }
    }
}
