using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace API.Services.MusicSearch;

public sealed class LavalinkSearchClient(HttpClient httpClient, IOptions<MusicSearchOptions> options)
{
    private const int MaxResponseBytes = 2 * 1024 * 1024;

    public async Task<JsonDocument?> GetAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Password))
            throw new MusicSearchException("SearchNotConfigured", "Music search is not configured.", 503);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutSeconds));
        using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
        request.Headers.Add("Authorization", options.Value.Password);
        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (response.StatusCode == HttpStatusCode.NoContent) return null;
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retry = response.Headers.RetryAfter;
                var seconds = retry?.Delta?.TotalSeconds ?? (retry?.Date - DateTimeOffset.UtcNow)?.TotalSeconds ?? 30;
                throw new MusicSearchException("ProviderRateLimited", "The provider is rate limited.", 429,
                    (int)Math.Clamp(Math.Ceiling(seconds), 1, 3600));
            }
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new MusicSearchException("ProviderUnavailable", "The search service cannot authenticate upstream.", 503);
            if (!response.IsSuccessStatusCode)
                throw new MusicSearchException("ProviderFailure", "The search provider request failed.");

            await response.Content.LoadIntoBufferAsync(MaxResponseBytes, timeout.Token);
            return JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(timeout.Token),
                new JsonDocumentOptions { MaxDepth = 32 });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MusicSearchException("ProviderTimeout", "The search provider timed out.", 504);
        }
        catch (HttpRequestException)
        {
            throw new MusicSearchException("ProviderUnavailable", "The search provider is unavailable.", 503);
        }
        catch (JsonException)
        {
            throw new MusicSearchException("InvalidProviderResponse", "The search provider returned invalid data.");
        }
    }
}
