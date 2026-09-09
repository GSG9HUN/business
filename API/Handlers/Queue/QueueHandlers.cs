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

        var payloadJson = JsonSerializer.Serialize(new QueueEnqueueCommandPayload(
            request.Query.Trim(),
            string.IsNullOrWhiteSpace(request.SearchMode) ? null : request.SearchMode.Trim()));

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

        await queueRepository.MarkAllQueuedAsSkippedAsync(guildId, cancellationToken);
        return HttpResults.Ok(new QueueResponse(guildId.ToString(), 0, []));
    }

    public static async Task<IResult> ShuffleAsync(
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
        if (queueItems.Count < 2)
        {
            return HttpResults.Conflict(new { ErrorMessage = "There are not enough tracks in the queue to shuffle." });
        }

        var shuffledIdentifiers = queueItems.Select(item => item.TrackIdentifier).ToList();
        Shuffle(shuffledIdentifiers);

        await queueRepository.ReorderQueuedItemsAsync(guildId, shuffledIdentifiers, cancellationToken);

        var updatedItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        return HttpResults.Ok(MapQueue(guildId, updatedItems));
    }

    private static QueueResponse MapQueue(ulong guildId, IReadOnlyList<QueueItemRecord> queueItems)
    {
        var tracks = new List<QueueTrackResponse>(queueItems.Count);
        var position = 1;
        foreach (var item in queueItems)
        {
            var mappedTrack = TrackResponseMapper.TryMapQueueTrack(item.TrackIdentifier, position);
            if (mappedTrack is null)
            {
               continue;
            }
            tracks.Add(mappedTrack);
            position++;
        }

        return new QueueResponse(guildId.ToString(), tracks.Count, tracks);
    }

    private static void Shuffle(IList<string> values)
    {
        var original = values.ToArray();

        for (var attempt = 0; attempt < 10; attempt++)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                var swapIndex = Random.Shared.Next(index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }

            if (!values.SequenceEqual(original))
            {
                return;
            }
        }
    }

    private sealed record QueueEnqueueCommandPayload(string Query, string? SearchMode);
}
