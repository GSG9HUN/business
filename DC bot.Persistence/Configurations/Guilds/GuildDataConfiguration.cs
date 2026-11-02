using DC_bot.Configurations.Shared;
using DC_bot.Entities.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Guilds;

/// <summary>
/// Configures PostgreSQL storage, keys, and relationships for guild data.
/// </summary>
public class GuildDataConfiguration : IEntityTypeConfiguration<GuildDataEntity>
{

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<GuildDataEntity> builder)
    {
        builder.ToTable("guild_data");
        
        builder.HasKey(entity => entity.GuildId);
        
        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.IsPremium)
            .HasColumnName("is_premium")
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(entity => entity.PremiumUntilUtc)
            .HasColumnName("premium_until_utc");
        
        builder.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();
        
        builder.Property(entity => entity.IsTwentyFourSevenEnabled)
            .HasColumnName("is_twenty_four_seven_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(entity => entity.PreferredVoiceChannelId)
            .HasColumnName("preferred_voice_channel_id")
            .HasColumnType("numeric(20,0)");

        builder.Property(entity => entity.PreferredTextChannelId)
            .HasColumnName("preferred_text_channel_id")
            .HasColumnType("numeric(20,0)");
    }
}
