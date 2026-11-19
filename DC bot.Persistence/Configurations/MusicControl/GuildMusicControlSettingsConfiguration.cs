using DC_bot.Configurations.Shared;
using DC_bot.Entities.MusicControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.MusicControl;

/// <summary>Maps the independent music-control preferences.</summary>
public sealed class GuildMusicControlSettingsConfiguration : IEntityTypeConfiguration<GuildMusicControlSettingsEntity>
{

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GuildMusicControlSettingsEntity> builder)
    {
        builder.ToTable("guild_music_control_settings");
        builder.HasKey(item => item.GuildId);
        builder.Property(item => item.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(item => item.VoteSkipEnabled)
            .HasColumnName("vote_skip_enabled")
            .HasDefaultValue(false);
        builder.Property(item => item.DjRoleId)
            .HasColumnName("dj_role_id")
            .HasConversion(value => (decimal?)value, value => (ulong?)value)
            .HasColumnType("numeric(20,0)");
    }
}
