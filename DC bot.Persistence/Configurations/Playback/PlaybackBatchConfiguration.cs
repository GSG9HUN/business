using DC_bot.Configurations.Shared;
using DC_bot.Entities.Playback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Playback;

/// <summary>
/// Configures PostgreSQL storage, keys, and relationships for playback batch.
/// </summary>
public class PlaybackBatchConfiguration : IEntityTypeConfiguration<PlaybackBatchEntity>
{

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PlaybackBatchEntity> builder)
    {
        builder.ToTable("playback_batches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");
        builder.Property(x => x.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage();
        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc");
        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc");
        builder.HasIndex(x => x.GuildId)
            .IsUnique()
            .HasFilter("completed_at_utc IS NULL");
        builder.HasIndex(x => new { x.GuildId, x.CreatedAtUtc });
        builder.HasOne(x => x.Guild)
            .WithMany()
            .HasForeignKey(x => x.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
