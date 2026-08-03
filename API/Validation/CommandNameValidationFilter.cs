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
        
        var normalizedCommand = commandName?.ToLowerInvariant();

        if (normalizedCommand is not ("pause" or "resume" or "skip" or "stop"))
        {
            return BadRequest(new
            {
                Code = ApiErrorCode.Validation,
                Message = "Invalid playback command.",
                Errors = new Dictionary<string, string[]>
                {
                    ["commandName"] = ["Allowed values: pause, resume, skip, stop."]
                }
            });
        }

        context.HttpContext.Items["commandName"] = normalizedCommand;
        return await next(context);
    }
}