using DC_bot.Configurations.Shared;
using DC_bot.Entities.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.MobileAppUserSettings;

public class MobileAppUserSettingsConfiguration : IEntityTypeConfiguration<MobileAppUserSettingsEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppUserSettingsEntity> builder)
    {
        builder.ToTable("mobile_app_user_settings");

        builder.HasKey(entity => entity.DiscordUserId);

        builder.Property(entity => entity.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();

        builder.Property(entity => entity.LanguageCode)
            .HasColumnName("language_code")
            .HasMaxLength(10)
            .HasDefaultValue(MobileAppLanguageCode.English)
            .IsRequired();

        builder.Property(entity => entity.Theme)
            .HasColumnName("theme")
            .HasMaxLength(20)
            .HasDefaultValue("system")
            .IsRequired();

        builder.Property(entity => entity.HapticFeedbackEnabled)
            .HasColumnName("haptic_feedback_enabled")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(entity => entity.SoundEffectsEnabled)
            .HasColumnName("sound_effects_enabled")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(entity => entity.TelemetryEnabled)
            .HasColumnName("telemetry_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(entity => entity.User)
            .WithOne(user => user.Settings)
            .HasForeignKey<MobileAppUserSettingsEntity>(entity => entity.DiscordUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.ToTable("mobile_app_user_settings", table =>
        {
            table.HasCheckConstraint(
                "ck_mobile_app_user_settings_theme",
                "theme IN ('dark', 'light', 'system')");
        });
    }
}