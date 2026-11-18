using DC_bot.Configurations.Shared;
using DC_bot.Entities.Discovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Discovery;

/// <summary>Maps durable discovery opt-in settings independently of runtime player state.</summary>
public sealed class UserMusicSettingsConfiguration : IEntityTypeConfiguration<UserMusicSettingsEntity>
{

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserMusicSettingsEntity> builder)
    {
        builder.ToTable("user_music_settings");
        builder.HasKey(item => item.UserId);
        builder.Property(item => item.UserId)
            .HasColumnName("user_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(item => item.SearchPickerEnabled)
            .HasColumnName("search_picker_enabled")
            .HasDefaultValue(false);
    }
}
