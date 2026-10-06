using API.Handlers.MusicSearch;
using API.Validation;

namespace API.Endpoints;

public static class MusicSearchEndpoints
{
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
