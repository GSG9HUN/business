using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using static Microsoft.AspNetCore.Http.Results;

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

        var userIdValue = httpContext.User.FindFirst("sub")?.Value;
        if (!ulong.TryParse(userIdValue, out var userId) || userId == 0)
        {
            return Unauthorized();
        }
        
        var hasAccess = await userRepository.HasGuildAccessAsync(userId, guildId, ct);
        if (!hasAccess)
        {
            return Forbid();
        }
        
        var command = await repository.EnqueueAsync(
            guildId,
            userId,
            commandName,
            ct);

        return Accepted($"/api/commands/{command.CommandId}", new
        {
            command.CommandId,
            command.State
        });
    }
}