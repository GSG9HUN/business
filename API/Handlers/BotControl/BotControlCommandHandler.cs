using API.Mapping;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.BotControl;

/// <summary>
/// Returns persisted command status only to the requesting user with guild access.
/// </summary>
public static class BotControlCommandHandler
{

    /// <summary>
    /// Loads command progress and rejects access from users other than the command owner.
    /// </summary>
    /// <param name="commandId">The command id value for this operation.</param>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="commandsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
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

        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, command.GuildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        return command.UserId == discordUserId ? HttpResults.Ok(BotControlCommandHttpMapper.ToStatusResponse(command)) : HttpResults.Forbid();
    }
}
