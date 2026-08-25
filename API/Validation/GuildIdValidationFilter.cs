using API.Errors;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

public sealed class GuildIdValidationFilter: IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeValues = context.HttpContext.Request.RouteValues;

        var guildIdString = routeValues["guildId"]?.ToString();
        if (string.IsNullOrEmpty(guildIdString) ||
            !ulong.TryParse(guildIdString, out var guildId) || 
            guildId == 0)
        {
            return BadRequest(new {
                Code = ApiErrorCode.Validation,
                Message = "Invalid request.",
                Errors = new Dictionary<string, string>
                {
                    ["guildId"] = "Invalid guild id."
                }
            });
        }
        
        context.HttpContext.Items["guildId"] = guildId;
        return await next(context);
    }
}