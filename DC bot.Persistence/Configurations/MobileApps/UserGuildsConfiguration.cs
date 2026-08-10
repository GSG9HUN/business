using DC_bot.Configurations.Shared;
using DC_bot.Entities.MobileApps;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.MobileApps;

public class UserGuildsConfiguration: IEntityTypeConfiguration<UserGuildEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserGuildEntity> builder)
    {
        builder.ToTable("user_guilds");

        builder.HasKey(entity => new { entity.DiscordUserId, entity.GuildId });

        builder.Property(entity => entity.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();

        builder.Property(entity => entity.Permissions)
            .HasColumnName("permissions")
            .HasGuildIdStorage()
            .IsRequired();

        builder.Property(entity => entity.IsOwner)
            .HasColumnName("is_owner")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(entity => entity.LastSeenAtUtc)
            .HasColumnName("last_seen_at_utc")
            .IsRequired()
            .HasDefaultValueSql("now()");
        
        builder.HasOne(entity => entity.User)
            .WithMany(user => user.Guilds)
            .HasForeignKey(entity => entity.DiscordUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(entity => entity.Guild)
            .WithMany(guild => guild.UserGuilds)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(entity => entity.GuildId);
    }
}