namespace API.Endpoints;

public static class QueueEndpoints
{
    public static RouteGroupBuilder MapQueueEndpoints(this RouteGroupBuilder group)
    {
      //  group.MapGet("guilds/{guildId}/queue", async (ulong guildId, IQueueFacade facade, CancellationToken ct) => await facade.GetAsync(guildId, ct));
      //  group.MapPost("guilds/{guildId}/queue/enqueue", async (ulong guildId, EnqueueRequest request, IQueueFacade facade, CancellationToken ct) => await facade.EnqueueAsync(guildId, request, ct));
      //  group.MapDelete("guilds/{guildId}/queue", async (ulong guildId, IQueueFacade facade, CancellationToken ct) => await facade.ClearAsync(guildId, ct));
      //  group.MapPost("guilds/{guildId}/queue/shuffle", async (ulong guildId, IQueueFacade facade, CancellationToken ct) => await facade.ShuffleAsync(guildId, ct));
        return group;
    }
}