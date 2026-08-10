using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DC_bot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMobileAppSessionColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RevokedAtUtc",
                table: "mobile_app_sessions",
                newName: "revoked_at_utc");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresAtUtc",
                table: "mobile_app_sessions",
                newName: "refresh_token_expires_at_utc");

            migrationBuilder.RenameColumn(
                name: "LastRefreshedAtUtc",
                table: "mobile_app_sessions",
                newName: "last_refreshed_at_utc");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "mobile_app_sessions",
                newName: "created_at_utc");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_refreshed_at_utc",
                table: "mobile_app_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at_utc",
                table: "mobile_app_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "revoked_at_utc",
                table: "mobile_app_sessions",
                newName: "RevokedAtUtc");

            migrationBuilder.RenameColumn(
                name: "refresh_token_expires_at_utc",
                table: "mobile_app_sessions",
                newName: "RefreshTokenExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_refreshed_at_utc",
                table: "mobile_app_sessions",
                newName: "LastRefreshedAtUtc");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "mobile_app_sessions",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastRefreshedAtUtc",
                table: "mobile_app_sessions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "mobile_app_sessions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
