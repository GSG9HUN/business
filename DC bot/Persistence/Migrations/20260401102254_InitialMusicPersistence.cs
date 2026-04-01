using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMusicPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guild_data",
                columns: table => new
                {
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    is_premium = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    premium_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_data", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "guild_playback_state",
                columns: table => new
                {
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    is_repeating = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_repeating_list = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    current_track_identifier = table.Column<string>(type: "text", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_playback_state", x => x.guild_id);
                    table.ForeignKey(
                        name: "FK_guild_playback_state_guild_data_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_premium_audit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    changed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    old_is_premium = table.Column<bool>(type: "boolean", nullable: false),
                    new_is_premium = table.Column<bool>(type: "boolean", nullable: false),
                    changed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_premium_audit", x => x.id);
                    table.ForeignKey(
                        name: "FK_guild_premium_audit_guild_data_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildQueueItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    TrackIdentifier = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<short>(type: "smallint", nullable: false),
                    AddedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SkippedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildQueueItems_guild_data_GuildId",
                        column: x => x.GuildId,
                        principalTable: "guild_data",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_premium_audit_guild_id_changed_at_utc",
                table: "guild_premium_audit",
                columns: new[] { "guild_id", "changed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_GuildQueueItems_GuildId",
                table: "GuildQueueItems",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_playback_state");

            migrationBuilder.DropTable(
                name: "guild_premium_audit");

            migrationBuilder.DropTable(
                name: "GuildQueueItems");

            migrationBuilder.DropTable(
                name: "guild_data");
        }
    }
}
