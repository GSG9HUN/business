using System.Text.Json;
using API.Requests.Queue;
using API.Responses.Queue;
using API.Mapping;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Queue;

public static class QueueHandlers
{
    public static async Task<IResult> GetAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IQueueRepository queueRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var queueItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        return HttpResults.Ok(MapQueue(guildId, queueItems));
    }

    public static async Task<IResult> EnqueueAsync(
        HttpContext httpContext,
        EnqueueRequest request,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Query is required." });
        }

        var user = await userRepository.GetUserAsync(discordUserId, cancellationToken);
        var requestedBy = string.IsNullOrWhiteSpace(user?.GlobalName)
            ? user?.Username
            : user.GlobalName;

        var payloadJson = JsonSerializer.Serialize(new QueueEnqueueCommandPayload(
            request.Query.Trim(),
            string.IsNullOrWhiteSpace(request.SearchMode) ? null : request.SearchMode.Trim(),
            requestedBy));

        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            "play",
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static async Task<IResult> ClearAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            "clear",
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static async Task<IResult> RemoveAsync(
        HttpContext httpContext,
        int trackNumber,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (trackNumber <= 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Track number must be greater than zero." });
        }

        var payloadJson = JsonSerializer.Serialize(new QueueRemoveCommandPayload(trackNumber));
        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            "remove",
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static async Task<IResult> ShuffleAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            "shuffle",
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static Task<IResult> MoveUpAsync(
        HttpContext httpContext,
        int trackIndex,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken) =>
        MoveAsync(httpContext, trackIndex, "moveUp", userRepository, commandsRepository, cancellationToken);

    public static Task<IResult> MoveDownAsync(
        HttpContext httpContext,
        int trackIndex,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken) =>
        MoveAsync(httpContext, trackIndex, "moveDown", userRepository, commandsRepository, cancellationToken);

    private static async Task<IResult> MoveAsync(
        HttpContext httpContext,
        int trackIndex,
        string commandType,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (trackIndex < 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Track index must be greater than or equal to zero." });
        }

        var payloadJson = JsonSerializer.Serialize(new QueueMoveCommandPayload(trackIndex));
        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            commandType,
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    private static QueueResponse MapQueue(ulong guildId, IReadOnlyList<QueueItemRecord> queueItems)
    {
        var tracks = new List<QueueTrackResponse>(queueItems.Count);
        var position = 1;
        foreach (var item in queueItems)
        {
            var mappedTrack = TrackResponseMapper.TryMapQueueTrack(item.TrackIdentifier, position, item.RequestedBy);
            if (mappedTrack is null)
            {
               continue;
            }
            tracks.Add(mappedTrack);
            position++;
        }

        return new QueueResponse(guildId.ToString(), tracks.Count, tracks);
    }

    private sealed record QueueEnqueueCommandPayload(string Query, string? SearchMode, string? RequestedBy);
    private sealed record QueueRemoveCommandPayload(int TrackNumber);
    private sealed record QueueMoveCommandPayload(int TrackIndex);
}
