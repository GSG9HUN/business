using API.Handlers.Guilds;

namespace API.Endpoints;

public static class GuildEndpoints
{
    public static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder group)
    {
        var guildGroup = group.MapGroup("/guilds").RequireAuthorization();
        guildGroup.MapGet("/", GuildHandler.Guilds);
        
        return group;
    }
}