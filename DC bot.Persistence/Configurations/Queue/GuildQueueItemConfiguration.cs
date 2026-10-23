using DC_bot.Configurations.Shared;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Queue;

/// <summary>
/// Configures PostgreSQL storage, keys, and relationships for guild queue item.
/// </summary>
public class GuildQueueItemConfiguration : IEntityTypeConfiguration<GuildQueueItemEntity>
{

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<GuildQueueItemEntity> builder)
    {
        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id");
        builder.Property(x => x.BatchItemId)
            .HasColumnName("batch_item_id");
        builder.HasIndex(x => new { x.BatchId, x.State, x.Position });
        builder.HasOne<DC_bot.Entities.Playback.PlaybackBatchEntity>()
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DC_bot.Entities.Playback.PlaybackBatchItemEntity>()
            .WithMany()
            .HasForeignKey(x => x.BatchItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("guild_queue_item");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();
        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .IsRequired();
        builder.Property(entity => entity.Position)
            .HasColumnName("position")
            .IsRequired();
        builder.Property(entity => entity.TrackIdentifier)
            .HasColumnName("track_identifier")
            .IsRequired();
        builder.Property(entity => entity.SourceQuery)
            .HasColumnName("source_query");
        builder.Property(entity => entity.SourceSearchMode)
            .HasColumnName("source_search_mode")
            .HasMaxLength(64);
        builder.Property(entity => entity.RequestedBy)
            .HasColumnName("requested_by")
            .HasMaxLength(128);
        builder.Property(entity => entity.State)
            .HasColumnName("state")
            .HasConversion<short>()
            .HasDefaultValue(QueueItemState.Queued)
            .IsRequired();
        builder.Property(entity => entity.AddedAtUtc)
            .HasColumnName("added_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();
        builder.Property(entity => entity.PlayedAtUtc)
            .HasColumnName("played_at_utc");
        builder.Property(entity => entity.SkippedAtUtc)
            .HasColumnName("skipped_at_utc");
        builder.HasIndex(entity => new { entity.GuildId, entity.Position })
            .IsUnique()
            .HasFilter("\"state\" IN (0, 4)");
        builder.Property(entity => entity.PlaybackOperationId)
            .HasColumnName("playback_operation_id");
        builder.Property(entity => entity.StartPreviousOperationId)
            .HasColumnName("start_previous_operation_id");
        builder.Property(entity => entity.StartPreviousState)
            .HasColumnName("start_previous_state")
            .HasConversion<short?>();
        builder.HasIndex(entity => entity.GuildId)
            .HasDatabaseName("ux_queue_starting_guild")
            .IsUnique()
            .HasFilter("\"state\" = 4");
        builder.Property(entity => entity.PlaybackNodeAddress)
            .HasColumnName("playback_node_address");
        builder.Property(entity => entity.PlaybackNodeSessionId)
            .HasColumnName("playback_node_session_id");
        builder.Property(entity => entity.PlaybackTransportConfirmed)
            .HasColumnName("playback_transport_confirmed")
            .HasDefaultValue(false);
        builder.HasIndex(entity => new { entity.GuildId, entity.State, entity.Position });
        builder.HasOne(entity => entity.Guild)
            .WithMany(guild => guild.QueueItems)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
