using API.Handlers.Guilds;
using API.Validation;

namespace API.Endpoints;

/// <summary>
/// Registers the guild HTTP routes and their authorization and validation filters.
/// </summary>
public static class GuildEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder group)
    {
        var guildGroup = group.MapGroup("/guilds")
            .RequireAuthorization();
        guildGroup.MapGet("/", GuildHandler.Guilds);
        guildGroup.MapGet("/{guildId}", GuildHandler.Guild)
            .AddEndpointFilter<GuildIdValidationFilter>();
        return group;
    }
}
