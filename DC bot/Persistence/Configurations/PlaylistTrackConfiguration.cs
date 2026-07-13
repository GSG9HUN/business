using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class PlaylistTrackConfiguration : IEntityTypeConfiguration<PlaylistTrackEntity>
{
    public void Configure(EntityTypeBuilder<PlaylistTrackEntity> builder)
    {
        builder.ToTable("playlist_tracks");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(entity => entity.PlaylistId)
            .HasColumnName("playlist_id")
            .IsRequired();

        builder.Property(entity => entity.TrackIdentifier)
            .HasColumnName("track_identifier")
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(entity => entity.OrderNumber)
            .HasColumnName("order_number")
            .IsRequired();

        builder.Property(entity => entity.Source)
            .HasColumnName("source")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(entity => entity.TrackUri)
            .HasColumnName("track_uri")
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasIndex(entity => new { entity.PlaylistId, entity.OrderNumber })
            .IsUnique();

        builder.HasOne(entity => entity.Playlist)
            .WithMany(entity => entity.Tracks)
            .HasForeignKey(entity => entity.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
