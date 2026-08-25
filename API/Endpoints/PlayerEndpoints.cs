namespace API.Endpoints;

public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this RouteGroupBuilder group)
    {
     //   group.MapGet("/guilds/{guildId}/player", async (ulong guildId, IPlaybackFacade facade, CancellationToken ct) => await facade.GetSnapshotAsync(guildId, ct));
        return group;
    }
}