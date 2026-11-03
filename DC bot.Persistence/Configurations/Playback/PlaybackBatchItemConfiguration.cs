using DC_bot.Entities.Playback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Playback;

/// <summary>
/// Configures PostgreSQL storage, keys, and relationships for playback batch item.
/// </summary>
public class PlaybackBatchItemConfiguration : IEntityTypeConfiguration<PlaybackBatchItemEntity>
{

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PlaybackBatchItemEntity> builder)
    {
        builder.ToTable("playback_batch_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");
        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id");
        builder.Property(x => x.Position)
            .HasColumnName("position");
        builder.Property(x => x.TrackIdentifier)
            .HasColumnName("track_identifier")
            .IsRequired();
        builder.Property(x => x.SourceQuery)
            .HasColumnName("source_query");
        builder.Property(x => x.SourceSearchMode)
            .HasColumnName("source_search_mode");
        builder.Property(x => x.RequestedBy)
            .HasColumnName("requested_by");
        builder.Property(x => x.AddedAtUtc)
            .HasColumnName("added_at_utc");
        builder.HasIndex(x => new { x.BatchId, x.Position })
            .IsUnique();
        builder.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
