using API.Handlers.Guilds;
using API.Validation;

namespace API.Endpoints;

public static class GuildEndpoints
{
    public static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder group)
    {
        var guildGroup = group.MapGroup("/guilds").RequireAuthorization();
        guildGroup.MapGet("/", GuildHandler.Guilds);
        guildGroup.MapGet("/{guildId}", GuildHandler.Guild)
            .AddEndpointFilter<GuildIdValidationFilter>();
        
        return group;
    }
}
