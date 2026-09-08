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
        queue.MapPost("/shuffle", QueueHandlers.ShuffleAsync);

        return group;
    }
}
