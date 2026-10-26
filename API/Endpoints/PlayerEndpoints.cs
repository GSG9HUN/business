using API.Handlers.Player;
using API.Validation;

namespace API.Endpoints;

/// <summary>
/// Registers the player HTTP routes and their authorization and validation filters.
/// </summary>
public static class PlayerEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapPlayerEndpoints(this RouteGroupBuilder group)
    {
        var player = group.MapGroup("/guilds/{guildId}/player")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();
        player.MapGet("", PlayerHandler.GetSnapshotAsync);
        return group;
    }
}
