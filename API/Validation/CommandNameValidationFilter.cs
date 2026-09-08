using API.Errors;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

public class CommandNameValidationFilter: IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeValues = context.HttpContext.Request.RouteValues;

        var commandName = routeValues["commandName"]?.ToString();
        //TODO majd kiszervezni a commandName-ket valahova.
        
        if (!TryNormalize(commandName, out var normalizedCommand))
        {
            return BadRequest(new
            {
                Code = ApiErrorCode.Validation,
                Message = "Invalid playback command.",
                Errors = new Dictionary<string, string[]>
                {
                    ["commandName"] = ["Allowed values: pause, resume, skip, leave, stop, repeat, repeatList."]
                }
            });
        }

        context.HttpContext.Items["commandName"] = normalizedCommand;
        return await next(context);
    }

    private static bool TryNormalize(string? commandName, out string normalizedCommand)
    {
        normalizedCommand = commandName?.Trim().ToLowerInvariant() switch
        {
            "pause" => "pause",
            "resume" => "resume",
            "skip" => "skip",
            "leave" => "leave",
            "stop" => "leave",
            "repeat" => "repeat",
            "repeatlist" => "repeatList",
            _ => string.Empty
        };

        return normalizedCommand.Length > 0;
    }
}
