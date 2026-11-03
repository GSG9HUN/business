using API.Errors;
using DC_bot.BotControl;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

/// <summary>
/// Normalizes supported playback command route values before invoking handlers.
/// </summary>
public class CommandNameValidationFilter : IEndpointFilter
{

    /// <summary>
    /// Rejects unsupported command names or stores the normalized command before calling the next filter.
    /// </summary>
    /// <param name="context">The context value for this operation.</param>
    /// <param name="next">The next value for this operation.</param>
    /// <returns>The computed invoke result.</returns>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeValues = context.HttpContext.Request.RouteValues;
        var commandName = routeValues["commandName"]?.ToString();

        if (!TryNormalize(commandName, out var normalizedCommand))
        {
            return BadRequest(new { Code = ApiErrorCode.Validation, Message = "Invalid playback command.", Errors = new Dictionary<string, string[]> { ["commandName"] = ["Allowed values: pause, resume, skip, previous, leave, stop, repeat, repeatList."] } });
        }

        context.HttpContext.Items["commandName"] = normalizedCommand;
        return await next(context);
    }

    /// <summary>
    /// Normalizes supported command names, including stop and repeat-list aliases.
    /// </summary>
    /// <param name="commandName">The command name value for this operation.</param>
    /// <param name="normalizedCommand">Receives the normalized command result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    private static bool TryNormalize(string? commandName, out string normalizedCommand)
    {
        normalizedCommand = commandName?.Trim()
            .ToLowerInvariant() switch
        {
            BotControlCommandTypes.Pause => BotControlCommandTypes.Pause,
            BotControlCommandTypes.Resume => BotControlCommandTypes.Resume,
            BotControlCommandTypes.Skip => BotControlCommandTypes.Skip,
            BotControlCommandTypes.Previous => BotControlCommandTypes.Previous,
            BotControlCommandTypes.Leave => BotControlCommandTypes.Leave,
            BotControlCommandTypes.Stop => BotControlCommandTypes.Leave,
            BotControlCommandTypes.Repeat => BotControlCommandTypes.Repeat,
            "repeatlist" => BotControlCommandTypes.RepeatList,
            _ => string.Empty
        };
        return normalizedCommand.Length > 0;
    }
}
