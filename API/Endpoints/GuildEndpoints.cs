namespace API.Endpoints;

public static class GuildEndpoints
{
    public static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder group)
    {
        //group.MapGet("/guilds", async (IGuildFacade facade, CancellationToken ct) => await facade.ListKnownGuildsAsync(ct));
        //group.MapGet("/guilds/{guildId}/status", async (ulong guildId, IGuildFacade facade, CancellationToken ct) => await facade.GetStatusAsync(guildId, ct));
        return group;
    }
}