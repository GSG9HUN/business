using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Configurations;

public class PlaylistConfiguration : IEntityTypeConfiguration<PlaylistEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PlaylistEntity> builder)
    {
        builder.ToTable("playlists");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .IsRequired();

        builder.HasIndex(entity => new { entity.GuildId, entity.Name }).IsUnique();

        builder.HasOne(entity => entity.Guild)
            .WithMany(entity => entity.Playlists)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
