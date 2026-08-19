using DC_bot.Configurations.Shared;
using DC_bot.Entities.BotRuntimeStatus;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Configurations.BotRuntimeStatus;

public class BotRuntimeStatusConfiguration : IEntityTypeConfiguration<BotRuntimeStatusEntity>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<BotRuntimeStatusEntity> builder)
    {
        builder.ToTable("bot_runtime_status");

        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.LastHeartbeatAtUtc)
            .HasColumnName("last_heartbeat_at_utc")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(entity => entity.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .IsRequired()
            .ValueGeneratedNever();
    }
}