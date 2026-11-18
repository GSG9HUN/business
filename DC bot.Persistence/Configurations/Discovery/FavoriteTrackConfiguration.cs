using DC_bot.Configurations.Shared;
using DC_bot.Entities.Discovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Discovery;

/// <summary>Maps personal favorites and unique source identities to PostgreSQL.</summary>
public sealed class FavoriteTrackConfiguration : IEntityTypeConfiguration<FavoriteTrackEntity>
{

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FavoriteTrackEntity> builder)
    {
        builder.ToTable("favorite_track");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();
        builder.Property(item => item.UserId)
            .HasColumnName("user_id")
            .HasGuildIdStorage();
        builder.Property(item => item.TrackKey)
            .HasColumnName("track_key")
            .HasMaxLength(64);
        builder.Property(item => item.TrackIdentifier)
            .HasColumnName("track_identifier");
        builder.Property(item => item.Title)
            .HasColumnName("title");
        builder.Property(item => item.Author)
            .HasColumnName("author");
        builder.Property(item => item.AddedAtUtc)
            .HasColumnName("added_at_utc");
        builder.HasIndex(item => new { item.UserId, item.TrackKey })
            .IsUnique();
    }
}
