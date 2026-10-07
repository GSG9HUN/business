using API.Responses.MusicSearch;
using API.Services.MusicSearch;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.MusicSearch;

public static class MusicSearchHandlers
{
    public static Task<IResult> GetCapabilitiesAsync(HttpContext context, IMobileAppUserRepository users,
        IMusicSearchService service, CancellationToken cancellationToken) =>
        AuthorizedAsync(context, users,
            async () => HttpResults.Ok(await service.GetCapabilitiesAsync(cancellationToken)), cancellationToken);

    public static Task<IResult> SearchAsync(HttpContext context, IMobileAppUserRepository users,
        IMusicSearchService service, CancellationToken cancellationToken) =>
        AuthorizedAsync(context, users, async () =>
        {
            foreach (var name in new[] { "provider", "kind", "query", "pageToken" })
                if (context.Request.Query[name].Count > 1)
                    throw new MusicSearchException("InvalidRequest", "Query parameters must not be repeated.", 400);

            string? Value(string name) =>
                context.Request.Query.TryGetValue(name, out var value) ? value.ToString() : null;

            return HttpResults.Ok(await service.SearchAsync(Value("provider"), Value("kind"), Value("query"),
                Value("pageToken"), cancellationToken));
        }, cancellationToken);

    private static async Task<IResult> AuthorizedAsync(HttpContext context, IMobileAppUserRepository users,
        Func<Task<IResult>> operation, CancellationToken cancellationToken)
    {
        var guildId = (ulong)context.Items["guildId"]!;
        var (_, error) = await ApiUserContext.RequireGuildAccessAsync(context, users, guildId, cancellationToken);
        if (error is not null) return error;
        context.Response.Headers.CacheControl = "no-store";
        try
        {
            return await operation();
        }
        catch (MusicSearchException ex)
        {
            if (ex.RetryAfterSeconds is { } retry)
                context.Response.Headers.RetryAfter = retry.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return HttpResults.Json(new MusicSearchErrorResponse(ex.Code, ex.Message), statusCode: ex.StatusCode);
        }
    }
}