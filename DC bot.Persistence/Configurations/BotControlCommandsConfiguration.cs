using DC_bot.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations;

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
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .IsRequired();
    }
}
