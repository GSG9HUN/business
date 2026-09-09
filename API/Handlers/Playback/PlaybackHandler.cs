using API.Mapping;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;

namespace API.Handlers.Playback;

public static class PlaybackHandlers
{
    public static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        IBotControlCommandsRepository repository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var commandName = (string)httpContext.Items["commandName"]!;

        var (userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            ct);
        if (accessError is not null)
        {
            return accessError;
        }
        
        var command = await repository.EnqueueAsync(
            guildId,
            userId,
            commandName,
            ct);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }
}
