using DC_bot.Configurations.BotControl;
using DC_bot.Configurations.BotRuntimeStatus;
using DC_bot.Configurations.GuildBotStatus;
using DC_bot.Configurations.Guilds;
using DC_bot.Configurations.MobileApps;
using DC_bot.Configurations.Playback;
using DC_bot.Configurations.Playlists;
using DC_bot.Configurations.Queue;
using DC_bot.Entities.BotControl;
using DC_bot.Entities.BotRuntimeStatus;
using DC_bot.Entities.GuildBotStatus;
using DC_bot.Entities.Guilds;
using DC_bot.Entities.MobileApps;
using DC_bot.Entities.Playback;
using DC_bot.Entities.Playlists;
using DC_bot.Entities.Queue;
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
    public DbSet<MobileAppUserEntity> MobileAppUsers => Set<MobileAppUserEntity>();
    public DbSet<UserGuildEntity> UserGuilds => Set<UserGuildEntity>();
    public DbSet<MobileAppSessionEntity> MobileAppSessions => Set<MobileAppSessionEntity>();
    public DbSet<BotRuntimeStatusEntity> BotRuntimeStatus => Set<BotRuntimeStatusEntity>();
    public DbSet<GuildBotStatusEntity> GuildBotStatus => Set<GuildBotStatusEntity>();
    
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
        modelBuilder.ApplyConfiguration(new MobileAppUsersConfiguration());
        modelBuilder.ApplyConfiguration(new MobileAppSessionsConfiguration());
        modelBuilder.ApplyConfiguration(new UserGuildsConfiguration());
        modelBuilder.ApplyConfiguration(new BotRuntimeStatusConfiguration());
        modelBuilder.ApplyConfiguration(new GuildBotStatusConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}