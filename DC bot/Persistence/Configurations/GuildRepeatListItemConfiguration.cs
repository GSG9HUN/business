using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class GuildRepeatListItemConfiguration : IEntityTypeConfiguration<GuildRepeatListItemEntity>
{
	public void Configure(EntityTypeBuilder<GuildRepeatListItemEntity> builder)
	{
		builder.ToTable("guild_repeat_list_item");

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

		builder.Property(entity => entity.AddedAtUtc)
			.HasColumnName("added_at_utc")
			.HasDefaultValueSql("now()")
			.IsRequired();

		builder.HasIndex(entity => new { entity.GuildId, entity.Position })
			.IsUnique();

		builder.HasOne(entity => entity.Guild)
			.WithMany(guild => guild.RepeatListItems)
			.HasForeignKey(entity => entity.GuildId)
			.OnDelete(DeleteBehavior.Cascade);
	}

}