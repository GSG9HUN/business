using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileAppUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_app_users",
                columns: table => new
                {
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    global_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    avatar_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_login_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_app_users", x => x.discord_user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_guilds",
                columns: table => new
                {
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    permissions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    is_owner = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_seen_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_guilds", x => new { x.discord_user_id, x.guild_id });
                    table.ForeignKey(
                        name: "FK_user_guilds_guild_data_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_guilds_mobile_app_users_discord_user_id",
                        column: x => x.discord_user_id,
                        principalTable: "mobile_app_users",
                        principalColumn: "discord_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_guilds_guild_id",
                table: "user_guilds",
                column: "guild_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_guilds");

            migrationBuilder.DropTable(
                name: "mobile_app_users");
        }
    }
}
