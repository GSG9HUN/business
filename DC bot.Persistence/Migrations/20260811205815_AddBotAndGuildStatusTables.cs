using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBotAndGuildStatusTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bot_runtime_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    last_heartbeat_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_runtime_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guild_bot_status",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    connected_voice_channel_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    connected_voice_user_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_bot_status", x => x.guild_id);
                    table.ForeignKey(
                        name: "FK_guild_bot_status_guild_data_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_runtime_status");

            migrationBuilder.DropTable(
                name: "guild_bot_status");
        }
    }
}
