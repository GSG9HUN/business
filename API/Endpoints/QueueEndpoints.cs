using API.Handlers.Queue;
using API.Validation;

namespace API.Endpoints;

/// <summary>
/// Registers the queue HTTP routes and their authorization and validation filters.
/// </summary>
public static class QueueEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapQueueEndpoints(this RouteGroupBuilder group)
    {
        var queue = group.MapGroup("/guilds/{guildId}/queue")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();
        queue.MapGet("", QueueHandlers.GetAsync);
        queue.MapPost("/enqueue", QueueHandlers.EnqueueAsync)
            .AddEndpointFilter<MusicEnqueueValidationFilter>();
        queue.MapDelete("", QueueHandlers.ClearAsync);
        queue.MapDelete("/{trackNumber:int}", QueueHandlers.RemoveAsync);
        queue.MapPost("/shuffle", QueueHandlers.ShuffleAsync);
        queue.MapPatch("/{trackIndex:int}/move-to/{targetIndex:int}", QueueHandlers.MoveToIndexAsync);
        return group;
    }
}
