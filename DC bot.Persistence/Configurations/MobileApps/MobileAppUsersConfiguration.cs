using DC_bot.Configurations.Shared;
using DC_bot.Entities.MobileApps;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.MobileApps;

public class MobileAppUsersConfiguration : IEntityTypeConfiguration<MobileAppUserEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppUserEntity> builder)
    {
        builder.ToTable("mobile_app_users");

        builder.HasKey(entity => entity.DiscordUserId);

        builder.Property(entity => entity.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();

        builder.Property(entity => entity.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.GlobalName)
            .HasColumnName("global_name")
            .HasMaxLength(100);

        builder.Property(entity => entity.AvatarHash)
            .HasColumnName("avatar_hash")
            .HasMaxLength(100);


        builder.Property(entity => entity.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(entity => entity.LastLoginAtUtc)
            .HasColumnName("last_login_at_utc")
            .IsRequired()
            .HasDefaultValueSql("now()");

        //builder.HasIndex(entity => new { entity.GuildId, entity.Name }).IsUnique();

        /*builder.HasOne(entity => entity.Guild)
            .WithMany(entity => entity.Playlists)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);*/
    }
}