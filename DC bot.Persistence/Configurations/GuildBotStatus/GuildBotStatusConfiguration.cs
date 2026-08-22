using DC_bot.Configurations.Shared;
using DC_bot.Entities.GuildBotStatus;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.GuildBotStatus;

public class GuildBotStatusConfiguration: IEntityTypeConfiguration<GuildBotStatusEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<GuildBotStatusEntity> builder)
    {
        builder.ToTable("guild_bot_status");

        builder.HasKey(entity => entity.GuildId);
        
        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.ConnectedVoiceChannelName)
            .HasColumnName("connected_voice_channel_name")
            .HasMaxLength(255)
            .ValueGeneratedNever();

        builder.Property(entity => entity.ConnectedVoiceUserCount)
            .HasColumnName("connected_voice_user_count")
            .IsRequired()
            .HasDefaultValue(0)
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever();
        
        builder.HasOne(entity => entity.Guild)
            .WithOne(guild => guild.BotStatus)
            .HasForeignKey<GuildBotStatusEntity>(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}