using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Db;

public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options)
{
    public DbSet<GuildDataEntity> GuildData => Set<GuildDataEntity>();
    public DbSet<GuildPlaybackStateEntity> GuildPlaybackStates => Set<GuildPlaybackStateEntity>();
    public DbSet<GuildQueueItemEntity> GuildQueueItems => Set<GuildQueueItemEntity>();
    public DbSet<GuildPremiumAuditEntity> GuildPremiumAudits => Set<GuildPremiumAuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.GuildDataConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GuildPlaybackStateConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GuildQueueItemConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GuildPremiumAuditConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}