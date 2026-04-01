using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class GuildQueueItemConfiguration : IEntityTypeConfiguration<GuildQueueItemEntity>
{
    public void Configure(EntityTypeBuilder<GuildQueueItemEntity> builder)
    {
        builder.ToTable("guild_queue_item");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .IsRequired();

        builder.Property(entity => entity.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(entity => entity.TrackIdentifier)
            .HasColumnName("track_identifier")
            .IsRequired();

        builder.Property(entity => entity.State)
            .HasColumnName("state")
            .HasDefaultValue((short)0)
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
            .IsUnique();

        builder.HasIndex(entity => new { entity.GuildId, entity.State, entity.Position });

        builder.HasOne(entity => entity.Guild)
            .WithMany(guild => guild.QueueItems)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
