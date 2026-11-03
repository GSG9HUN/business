using DC_bot.Configurations.BotControl;
using DC_bot.Configurations.GuildBotStatus;
using DC_bot.Configurations.Guilds;
using DC_bot.Configurations.MobileApps;
using DC_bot.Configurations.MobileAppUserSettings;
using DC_bot.Configurations.Playback;
using DC_bot.Configurations.Playlists;
using DC_bot.Configurations.Queue;
using DC_bot.Entities.BotControl;
using DC_bot.Entities.GuildBotStatus;
using DC_bot.Entities.Guilds;
using DC_bot.Entities.MobileApps;
using DC_bot.Entities.MobileAppUserSettings;
using DC_bot.Entities.Playback;
using DC_bot.Entities.Playlists;
using DC_bot.Entities.Queue;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Db;

/// <summary>
/// Owns the shared EF Core model and persisted guild, queue, playback, playlist, and mobile state.
/// </summary>
/// <param name = "options">The options value used by this storage contract.</param>
public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options)
{

    /// <summary>
    /// The set of persisted Discord guild records.
    /// </summary>
    public DbSet<GuildDataEntity> GuildData => Set<GuildDataEntity>();

    /// <summary>
    /// The set of persisted guild playback states.
    /// </summary>
    public DbSet<GuildPlaybackStateEntity> GuildPlaybackStates => Set<GuildPlaybackStateEntity>();

    /// <summary>
    /// The set of persisted guild queue entries.
    /// </summary>
    public DbSet<GuildQueueItemEntity> GuildQueueItems => Set<GuildQueueItemEntity>();

    /// <summary>
    /// The set of canonical playback batches, including completed history entries.
    /// </summary>
    public DbSet<PlaybackBatchEntity> PlaybackBatches => Set<PlaybackBatchEntity>();

    /// <summary>
    /// The set of ordered canonical tracks belonging to playback batches.
    /// </summary>
    public DbSet<PlaybackBatchItemEntity> PlaybackBatchItems => Set<PlaybackBatchItemEntity>();

    /// <summary>
    /// The set of persisted guild repeat-list snapshot entries.
    /// </summary>
    public DbSet<GuildRepeatListItemEntity> GuildRepeatListItems => Set<GuildRepeatListItemEntity>();

    /// <summary>
    /// The set of audit records for guild premium status changes.
    /// </summary>
    public DbSet<GuildPremiumAuditEntity> GuildPremiumAudits => Set<GuildPremiumAuditEntity>();

    /// <summary>
    /// The set of saved playlists across all guilds.
    /// </summary>
    public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();

    /// <summary>
    /// The set of tracks belonging to saved playlists.
    /// </summary>
    public DbSet<PlaylistTrackEntity> PlaylistTracks => Set<PlaylistTrackEntity>();

    /// <summary>
    /// The set of durable commands and execution results shared with the bot.
    /// </summary>
    public DbSet<BotControlCommandEntity> BotControlCommands => Set<BotControlCommandEntity>();

    /// <summary>
    /// The set of Discord users registered with the mobile application.
    /// </summary>
    public DbSet<MobileAppUserEntity> MobileAppUsers => Set<MobileAppUserEntity>();

    /// <summary>
    /// The set of persisted user-to-guild memberships.
    /// </summary>
    public DbSet<UserGuildEntity> UserGuilds => Set<UserGuildEntity>();

    /// <summary>
    /// The set of realtime connections subscribed to guild groups.
    /// </summary>
    public DbSet<MobileRealtimeGuildMembershipEntity> MobileRealtimeGuildMemberships => Set<MobileRealtimeGuildMembershipEntity>();

    /// <summary>
    /// The set of mobile application authentication sessions.
    /// </summary>
    public DbSet<MobileAppSessionEntity> MobileAppSessions => Set<MobileAppSessionEntity>();

    /// <summary>
    /// The set of persisted bot voice statuses, keyed by guild.
    /// </summary>
    public DbSet<GuildBotStatusEntity> GuildBotStatus => Set<GuildBotStatusEntity>();

    /// <summary>
    /// The set of persisted mobile application preferences, keyed by user.
    /// </summary>
    public DbSet<MobileAppUserSettingsEntity> MobileAppUserSettings => Set<MobileAppUserSettingsEntity>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuildDataConfiguration());
        modelBuilder.ApplyConfiguration(new GuildPlaybackStateConfiguration());
        modelBuilder.ApplyConfiguration(new GuildQueueItemConfiguration());
        modelBuilder.ApplyConfiguration(new PlaybackBatchConfiguration());
        modelBuilder.ApplyConfiguration(new PlaybackBatchItemConfiguration());
        modelBuilder.ApplyConfiguration(new GuildRepeatListItemConfiguration());
        modelBuilder.ApplyConfiguration(new GuildPremiumAuditConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistTrackConfiguration());
        modelBuilder.ApplyConfiguration(new BotControlCommandsConfiguration());
        modelBuilder.ApplyConfiguration(new MobileAppUsersConfiguration());
        modelBuilder.ApplyConfiguration(new MobileAppUserSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new MobileAppSessionsConfiguration());
        modelBuilder.ApplyConfiguration(new UserGuildsConfiguration());
        modelBuilder.ApplyConfiguration(new MobileRealtimeGuildMembershipsConfiguration());
        modelBuilder.ApplyConfiguration(new GuildBotStatusConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
