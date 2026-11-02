using System.Security.Claims;
using DC_bot.Interface.Service.Persistence.MobileApps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Realtime;

/// <summary>
/// Manages authenticated mobile subscriptions and their persisted guild memberships.
/// </summary>
/// <param name="mobileAppUserRepository">The injected persistence contract used by this operation.</param>
/// <param name="mobileRealtimeMembershipRepository">The injected persistence contract used by this operation.</param>
/// <param name="logger">The logger for lifecycle and failure diagnostics.</param>
[Authorize]
public sealed class MobileUpdatesHub(
    IMobileAppUserRepository mobileAppUserRepository,
    IMobileRealtimeMembershipRepository mobileRealtimeMembershipRepository,
    ILogger<MobileUpdatesHub> logger) : Hub
{

    /// <summary>
    /// Logs the authenticated hub connection and delegates to the SignalR connection lifecycle.
    /// </summary>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public override Task OnConnectedAsync()
    {
        var userId = TryGetDiscordUserId();

        if (userId.HasValue)
        {
            logger.LogInformation("Mobile realtime client connected. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId.Value);
        }
        else
        {
            logger.LogWarning("Mobile realtime client connected without a valid user id. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Removes persisted subscriptions independently of the aborted connection before completing disconnection.
    /// </summary>
    /// <param name="exception">The exception value for this operation.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = TryGetDiscordUserId();

        if (exception is null)
        {
            logger.LogInformation("Mobile realtime client disconnected. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        }
        else
        {
            logger.LogWarning(exception, "Mobile realtime client disconnected with an error. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        }

        await mobileRealtimeMembershipRepository.RemoveConnectionMembershipsAsync(Context.ConnectionId, CancellationToken.None);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Checks persisted guild access, joins the user-guild SignalR group, and stores the subscription.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public async Task JoinGuildAsync(string guildId)
    {
        var userId = GetDiscordUserId();

        if (!ulong.TryParse(guildId, out var parsedGuildId))
        {
            logger.LogWarning("Mobile realtime guild join rejected because the guild id was invalid. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, guildId);
            throw new HubException("Invalid guild id.");
        }

        logger.LogInformation("Mobile realtime guild join requested. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, parsedGuildId);
        var hasAccess = await mobileAppUserRepository.HasGuildAccessAsync(userId, parsedGuildId, Context.ConnectionAborted);

        if (!hasAccess)
        {
            logger.LogWarning("Mobile realtime guild join rejected because the user has no guild access. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, parsedGuildId);
            throw new HubException("Guild access denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, MobileUpdateGroups.UserGuild(parsedGuildId, userId), Context.ConnectionAborted);
        await mobileRealtimeMembershipRepository.UpsertGuildMembershipAsync(Context.ConnectionId, userId, parsedGuildId, Context.ConnectionAborted);
        logger.LogInformation("Mobile realtime guild join accepted. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, parsedGuildId);
    }

    /// <summary>
    /// Removes the connection&apos;s user-guild SignalR group and persisted subscription.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public async Task LeaveGuildAsync(string guildId)
    {
        var userId = GetDiscordUserId();

        if (!ulong.TryParse(guildId, out var parsedGuildId))
        {
            logger.LogWarning("Mobile realtime guild leave ignored because the guild id was invalid. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, guildId);
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, MobileUpdateGroups.UserGuild(parsedGuildId, userId), Context.ConnectionAborted);
        await mobileRealtimeMembershipRepository.RemoveGuildMembershipAsync(Context.ConnectionId, userId, parsedGuildId, Context.ConnectionAborted);
        logger.LogInformation("Mobile realtime guild leave completed. ConnectionId: {ConnectionId}, UserId: {UserId}, GuildId: {GuildId}", Context.ConnectionId, userId, parsedGuildId);
    }

    /// <summary>
    /// Adds this authenticated connection to its per-user SignalR update group.
    /// </summary>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public async Task JoinUserUpdatesAsync()
    {
        var userId = GetDiscordUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, MobileUpdateGroups.User(userId), Context.ConnectionAborted);
        logger.LogInformation("Mobile realtime user updates join completed. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
    }

    /// <summary>
    /// Removes this authenticated connection from its per-user SignalR update group.
    /// </summary>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    public async Task LeaveUserUpdatesAsync()
    {
        var userId = GetDiscordUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, MobileUpdateGroups.User(userId), Context.ConnectionAborted);
        logger.LogInformation("Mobile realtime user updates leave completed. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
    }

    /// <summary>
    /// Reads the authenticated Discord user identifier or rejects an invalid hub identity.
    /// </summary>
    /// <returns>The computed get discord user id result.</returns>
    private ulong GetDiscordUserId()
    {
        return TryGetDiscordUserId() ?? throw new HubException("Invalid authenticated user.");
    }

    /// <summary>
    /// Tries to read the authenticated Discord user identifier from the subject claim.
    /// </summary>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private ulong? TryGetDiscordUserId()
    {
        var sub = Context.User?.FindFirstValue("sub");
        return ulong.TryParse(sub, out var userId) ? userId : null;
    }
}
