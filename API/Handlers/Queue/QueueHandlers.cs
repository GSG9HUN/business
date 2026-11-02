using System.Text.Json;
using API.Requests.Queue;
using API.Mapping;
using DC_bot.BotControl;
using DC_bot.BotControl.Payloads;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Queue;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Queue;

/// <summary>
/// Reads authorized guild queues and submits durable bot commands for queue mutations.
/// </summary>
public static class QueueHandlers
{

    /// <summary>
    /// Returns the authorized guild&apos;s currently queued, decodable tracks.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="queueRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> GetAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IQueueRepository queueRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(_, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        var queueItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        return HttpResults.Ok(QueueResponseMapper.Map(guildId, queueItems));
    }

    /// <summary>
    /// Validates the enqueue request and optional channel identifiers and persists a bot play command.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="request">The client request to validate and process.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> EnqueueAsync(
        HttpContext httpContext,
        EnqueueRequest request,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Query is required." });
        }

        var user = await userRepository.GetUserAsync(discordUserId, cancellationToken);
        var requestedBy = string.IsNullOrWhiteSpace(user?.GlobalName) ? user?.Username : user.GlobalName;

        if (
            !TryParseOptionalChannelId(
            request.VoiceChannelId,
            out var voiceChannelId))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "VoiceChannelId must be a valid Discord snowflake." });
        }

        if (
            !TryParseOptionalChannelId(
            request.TextChannelId,
            out var textChannelId))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "TextChannelId must be a valid Discord snowflake." });
        }

        var payloadJson = JsonSerializer.Serialize(new QueueEnqueueCommandPayload(request.Query.Trim(), string.IsNullOrWhiteSpace(request.SearchMode) ? null : request.SearchMode.Trim(), requestedBy, voiceChannelId, textChannelId));
        var command = await commandsRepository.EnqueueAsync(guildId, discordUserId, BotControlCommandTypes.Play, payloadJson, cancellationToken);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Queues a durable clear command for an authorized guild.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> ClearAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        var command = await commandsRepository.EnqueueAsync(guildId, discordUserId, BotControlCommandTypes.Clear, cancellationToken);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Validates a one-based track number and queues a durable remove command.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="trackNumber">The track number value for this operation.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> RemoveAsync(
        HttpContext httpContext,
        int trackNumber,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        if (trackNumber <= 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Track number must be greater than zero." });
        }

        var payloadJson = JsonSerializer.Serialize(new QueueRemoveCommandPayload(trackNumber));
        var command = await commandsRepository.EnqueueAsync(guildId, discordUserId, BotControlCommandTypes.Remove, payloadJson, cancellationToken);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Queues a durable shuffle command for an authorized guild.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> ShuffleAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        var command = await commandsRepository.EnqueueAsync(guildId, discordUserId, BotControlCommandTypes.Shuffle, cancellationToken);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Validates nonnegative source and target indices and queues a durable move command.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="trackIndex">The track index value for this operation.</param>
    /// <param name="targetIndex">The target index value for this operation.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> MoveToIndexAsync(
        HttpContext httpContext,
        int trackIndex,
        int targetIndex,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        if (trackIndex < 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Track index must be greater than or equal to zero." });
        }

        if (targetIndex < 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Target index must be greater than or equal to zero." });
        }

        var payloadJson = JsonSerializer.Serialize(new QueueMoveToIndexCommandPayload(trackIndex, targetIndex));
        var command = await commandsRepository.EnqueueAsync(guildId, discordUserId, BotControlCommandTypes.MoveToIndex, payloadJson, cancellationToken);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Parses an optional channel identifier, treating an empty value as unspecified.
    /// </summary>
    /// <param name="value">The value value for this operation.</param>
    /// <param name="channelId">Receives the channel id result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    private static bool TryParseOptionalChannelId(string? value, out ulong? channelId)
    {
        channelId = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!ulong.TryParse(value.Trim(), out var parsedChannelId))
        {
            return false;
        }

        channelId = parsedChannelId;
        return true;
    }
}
