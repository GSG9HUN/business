using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileRealtimeGuildMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_realtime_guild_memberships",
                columns: table => new
                {
                    connection_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    joined_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_realtime_guild_memberships", x => new { x.connection_id, x.guild_id });
                    table.ForeignKey(
                        name: "FK_mobile_realtime_guild_memberships_user_guilds_discord_user_~",
                        columns: x => new { x.discord_user_id, x.guild_id },
                        principalTable: "user_guilds",
                        principalColumns: new[] { "discord_user_id", "guild_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_realtime_guild_memberships_connection_id",
                table: "mobile_realtime_guild_memberships",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_realtime_guild_memberships_discord_user_id_guild_id",
                table: "mobile_realtime_guild_memberships",
                columns: new[] { "discord_user_id", "guild_id" });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_realtime_guild_memberships_guild_id_discord_user_id",
                table: "mobile_realtime_guild_memberships",
                columns: new[] { "guild_id", "discord_user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_realtime_guild_memberships");
        }
    }
}
