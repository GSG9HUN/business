using API.Handlers.Queue;
using API.Validation;

namespace API.Endpoints;

public static class QueueEndpoints
{
    public static RouteGroupBuilder MapQueueEndpoints(this RouteGroupBuilder group)
    {
        var queue = group
            .MapGroup("/guilds/{guildId}/queue")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();

        queue.MapGet("", QueueHandlers.GetAsync);
        queue.MapPost("/enqueue", QueueHandlers.EnqueueAsync);
        queue.MapDelete("", QueueHandlers.ClearAsync);
        queue.MapDelete("/{trackNumber:int}", QueueHandlers.RemoveAsync);
        queue.MapPost("/shuffle", QueueHandlers.ShuffleAsync);
        queue.MapPatch("/{trackIndex:int}/move-up", QueueHandlers.MoveUpAsync);
        queue.MapPatch("/{trackIndex:int}/move-down", QueueHandlers.MoveDownAsync);

        return group;
    }
}
