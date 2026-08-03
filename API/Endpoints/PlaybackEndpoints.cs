using API.Errors;
using API.Handlers;
using API.Validation;
using DC_bot.Interface.Service.Persistence;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Endpoints;

public static class PlaybackEndpoints
{
    public static RouteGroupBuilder MapPlaybackEndpoints(this RouteGroupBuilder group)
    {
        var playback = group
            .MapGroup("/guilds/{guildId}/playback")
            .AddEndpointFilter<GuildIdValidationFilter>();
        
        playback.MapPost("/{commandName}", PlaybackHandlers.ExecuteAsync)
            .AddEndpointFilter<CommandNameValidationFilter>();
        
        return group;
    }
}
