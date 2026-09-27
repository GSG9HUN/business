using API.Requests.Queue;
using API.Responses.MusicSearch;
using API.Services.MusicSearch;
using DC_bot.Music;
using Microsoft.Extensions.Options;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

public sealed class MusicEnqueueValidationFilter(IOptions<MusicSearchOptions> options) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<EnqueueRequest>().Single();
        var query = request.Query?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length > 2048 || query.Any(char.IsControl))
            return HttpResults.BadRequest(new MusicSearchErrorResponse("InvalidPlayInput",
                "A valid music query or provider URL is required."));

        MusicSearchProvider? FindMode(string? value) => MusicSearchProviders.Find(
                                                            string.Equals(value, "youtubemusic",
                                                                StringComparison.OrdinalIgnoreCase)
                                                                ? "ytmsearch"
                                                                : value) ??
                                                        MusicSearchProviders.All.FirstOrDefault(p =>
                                                            string.Equals(p.SourceName, value,
                                                                StringComparison.OrdinalIgnoreCase));

        bool CanEnqueue(MusicSearchProvider provider) =>
            options.Value.Providers.TryGetValue(provider.Id, out var setting) && setting.CanEnqueue;

        IResult Disabled() =>
            HttpResults.Json(new MusicSearchErrorResponse("EnqueueUnavailable", "Queueing this provider is disabled."),
                statusCode: 422);

        if (FindMode(request.SearchMode?.Trim()) is { } requestedProvider && !CanEnqueue(requestedProvider))
            return Disabled();
        var separator = query.IndexOf(':');
        if (separator > 0 && FindMode(query[..separator]) is { } prefixedProvider)
        {
            if (!CanEnqueue(prefixedProvider)) return Disabled();
            query = query[(separator + 1)..].Trim();
        }

        if (Uri.TryCreate(query, UriKind.Absolute, out _))
        {
            var providers = MusicSearchProviders.All.Where(p => MusicSearchProviders.IsProviderUrl(p.Id, query))
                .ToArray();
            if (providers.Length == 0)
                return HttpResults.BadRequest(new MusicSearchErrorResponse("InvalidProviderUrl",
                    "Use a supported provider content URL."));
            if (!providers.Any(CanEnqueue)) return Disabled();
        }

        return await next(context);
    }
}