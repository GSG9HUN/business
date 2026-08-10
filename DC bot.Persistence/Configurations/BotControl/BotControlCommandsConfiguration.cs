using DC_bot.Configurations.Shared;
using DC_bot.Entities.BotControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.BotControl;

public class BotControlCommandsConfiguration : IEntityTypeConfiguration<BotControlCommandEntity>
{
    public void Configure(EntityTypeBuilder<BotControlCommandEntity> builder)
    {
        builder.ToTable("bot_control_commands");

        builder.HasKey(entity => entity.CommandId);

        builder.Property(entity => entity.CommandId)
            .HasColumnName("command_id")
            .IsRequired();

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")    
            .HasGuildIdStorage()
            .IsRequired();
        
        builder.Property(entity => entity.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(entity => entity.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(entity => entity.ClaimedAtUtc)
            .HasColumnName("claimed_at_utc");

        builder.Property(entity => entity.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.HasIndex(entity => entity.Status);
    }
}
