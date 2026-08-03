using DC_bot.Configurations;
using DC_bot.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Db;

public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options)
{
    public DbSet<GuildDataEntity> GuildData => Set<GuildDataEntity>();
    public DbSet<GuildPlaybackStateEntity> GuildPlaybackStates => Set<GuildPlaybackStateEntity>();
    public DbSet<GuildQueueItemEntity> GuildQueueItems => Set<GuildQueueItemEntity>();
    public DbSet<GuildRepeatListItemEntity> GuildRepeatListItems => Set<GuildRepeatListItemEntity>();
    public DbSet<GuildPremiumAuditEntity> GuildPremiumAudits => Set<GuildPremiumAuditEntity>();
    public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();
    public DbSet<PlaylistTrackEntity> PlaylistTracks => Set<PlaylistTrackEntity>();
    public DbSet<BotControlCommandEntity> BotControlCommands => Set<BotControlCommandEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuildDataConfiguration());
        modelBuilder.ApplyConfiguration(new GuildPlaybackStateConfiguration());
        modelBuilder.ApplyConfiguration(new GuildQueueItemConfiguration());
        modelBuilder.ApplyConfiguration(new GuildRepeatListItemConfiguration());
        modelBuilder.ApplyConfiguration(new GuildPremiumAuditConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistTrackConfiguration());
        modelBuilder.ApplyConfiguration(new BotControlCommandsConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}