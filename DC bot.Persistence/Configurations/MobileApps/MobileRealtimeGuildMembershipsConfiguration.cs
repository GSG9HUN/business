using DC_bot.Configurations.Shared;
using DC_bot.Entities.MobileApps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.MobileApps;

/// <summary>
/// Configures PostgreSQL storage, keys, and relationships for mobile realtime guild memberships.
/// </summary>
public class MobileRealtimeGuildMembershipsConfiguration : IEntityTypeConfiguration<MobileRealtimeGuildMembershipEntity>
{

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MobileRealtimeGuildMembershipEntity> builder)
    {
        builder.ToTable("mobile_realtime_guild_memberships");
        builder.HasKey(entity => new { entity.ConnectionId, entity.GuildId });
        builder.Property(entity => entity.ConnectionId)
            .HasColumnName("connection_id")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(entity => entity.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(entity => entity.JoinedAtUtc)
            .HasColumnName("joined_at_utc")
            .IsRequired()
            .HasDefaultValueSql("now()");
        builder.HasIndex(entity => new { entity.GuildId, entity.DiscordUserId });
        builder.HasIndex(entity => entity.ConnectionId);
        builder.HasOne<UserGuildEntity>()
            .WithMany()
            .HasForeignKey(entity => new { entity.DiscordUserId, entity.GuildId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
