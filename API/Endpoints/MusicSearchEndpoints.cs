using API.Handlers.MusicSearch;
using API.Validation;

namespace API.Endpoints;

/// <summary>
/// Registers the music search HTTP routes and their authorization and validation filters.
/// </summary>
public static class MusicSearchEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapMusicSearchEndpoints(this RouteGroupBuilder group)
    {
        var search = group.MapGroup("/guilds/{guildId}/music-search")
            .RequireAuthorization()
            .RequireRateLimiting("music-search")
            .AddEndpointFilter<GuildIdValidationFilter>();
        search.MapGet("/capabilities", MusicSearchHandlers.GetCapabilitiesAsync);
        search.MapGet("", MusicSearchHandlers.SearchAsync);
        return group;
    }
}
