using API.Handlers.Player;
using API.Validation;

namespace API.Endpoints;

public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this RouteGroupBuilder group)
    {
        var player = group
            .MapGroup("/guilds/{guildId}/player")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();

        player.MapGet("", PlayerHandler.GetSnapshotAsync);

        return group;
    }
}
