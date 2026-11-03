using API.Handlers.Playback;
using API.Validation;

namespace API.Endpoints;

/// <summary>
/// Registers the playback HTTP routes and their authorization and validation filters.
/// </summary>
public static class PlaybackEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapPlaybackEndpoints(this RouteGroupBuilder group)
    {
        var playback = group.MapGroup("/guilds/{guildId}/playback")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();
        playback.MapPost("/{commandName}", PlaybackHandlers.ExecuteAsync)
            .AddEndpointFilter<CommandNameValidationFilter>();
        playback.MapPatch("/repeat-mode", PlaybackHandlers.SetRepeatModeAsync);
        return group;
    }
}
