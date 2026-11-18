using DC_bot.Configurations.Shared;
using DC_bot.Entities.Discovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Discovery;

/// <summary>Maps durable discovery opt-in settings independently of runtime player state.</summary>
public sealed class GuildMusicSettingsConfiguration : IEntityTypeConfiguration<GuildMusicSettingsEntity>
{

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GuildMusicSettingsEntity> builder)
    {
        builder.ToTable("guild_music_settings");
        builder.HasKey(item => item.GuildId);
        builder.Property(item => item.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(item => item.AutoplayEnabled)
            .HasColumnName("autoplay_enabled")
            .HasDefaultValue(false);
    }
}
