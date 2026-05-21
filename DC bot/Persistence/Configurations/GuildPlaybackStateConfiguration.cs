using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class GuildPlaybackStateConfiguration : IEntityTypeConfiguration<GuildPlaybackStateEntity>
{
    public void Configure(EntityTypeBuilder<GuildPlaybackStateEntity> builder)
    {
        builder.ToTable("guild_playback_state");

        builder.HasKey(entity => entity.GuildId);

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .ValueGeneratedNever();

        builder.Property(entity => entity.IsRepeating)
            .HasColumnName("is_repeating")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(entity => entity.IsRepeatingList)
            .HasColumnName("is_repeating_list")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(entity => entity.CurrentTrackIdentifier)
            .HasColumnName("current_track_identifier");

        builder.Property(entity => entity.QueueItemId)
            .HasColumnName("queue_item_id");

        builder.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(entity => entity.Guild)
            .WithOne(guild => guild.PlaybackState)
            .HasForeignKey<GuildPlaybackStateEntity>(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
