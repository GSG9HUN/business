using DC_bot.Configurations.Shared;
using DC_bot.Entities.MobileApps;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.MobileApps;

public class MobileAppSessionsConfiguration : IEntityTypeConfiguration<MobileAppSessionEntity>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppSessionEntity> builder)
    {
        builder.ToTable("mobile_app_sessions");
        builder.HasKey(x => x.SessionId);

        builder.Property(x => x.SessionId).HasColumnName("session_id");
        builder.Property(x => x.DiscordUserId).HasColumnName("discord_user_id").HasGuildIdStorage().IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("now()");
        builder.Property(x => x.LastRefreshedAtUtc).HasColumnName("last_refreshed_at_utc").HasDefaultValueSql("now()");
        builder.Property(x => x.RefreshTokenExpiresAtUtc).HasColumnName("refresh_token_expires_at_utc").IsRequired();
        builder.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => x.DiscordUserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.DiscordUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}