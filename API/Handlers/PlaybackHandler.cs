using DC_bot.Interface.Service.Persistence;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Handlers;

public static class PlaybackHandlers
{
    public static async Task<IResult> ExecuteAsync(
        string commandName,
        HttpContext httpContext,
        IBotControlCommandsRepository repository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;

        // TODO: auth claimből
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