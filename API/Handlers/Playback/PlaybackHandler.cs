using DC_bot.Interface.Service.Persistence.BotControl;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playback;

public static class PlaybackHandlers
{
    public static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        IBotControlCommandsRepository repository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var commandName = (string)httpContext.Request.Query["command"]!;

        // TODO: auth claimb�l
        const ulong userId = 0;

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