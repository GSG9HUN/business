using API.Mapping;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.BotControl;

public static class BotControlCommandHandler
{
    public static async Task<IResult> GetStatusAsync(
        string commandId,
        HttpContext httpContext,
        IBotControlCommandsRepository commandsRepository,
        IMobileAppUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandId))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Invalid command id." });
        }

        var command = await commandsRepository.GetByCommandIdAsync(commandId.Trim(), cancellationToken);
        if (command is null)
        {
            return HttpResults.NotFound(new { ErrorMessage = "Command was not found." });
        }

        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            command.GuildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        return command.UserId == discordUserId
            ? HttpResults.Ok(BotControlCommandHttpMapper.ToStatusResponse(command))
            : HttpResults.Forbid();
    }
}
