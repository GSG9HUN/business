using System.Net;
using System.Text.Json;
using Lavalink4NET;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle;

public sealed class PlaybackRemoteProbe(
    IHttpClientFactory httpClientFactory,
    IOptions<AudioServiceOptions> nodeOptions)
{
    public string NodeAddress => nodeOptions.Value.BaseAddress.GetLeftPart(UriPartial.Path).TrimEnd('/');

    private async Task<HttpResponseMessage> GetAsync(string path, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("PlaybackRecovery");
        using var request = new HttpRequestMessage(HttpMethod.Get, NodeAddress + path);
        request.Headers.TryAddWithoutValidation("Authorization", nodeOptions.Value.Passphrase);
        
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public async Task<bool> IsSessionGoneAsync(string nodeAddress, string sessionId, CancellationToken cancellationToken)
    {
        if (!string.Equals(nodeAddress, NodeAddress, StringComparison.Ordinal))
        {
            return false;
        }

        // Verify that the configured origin actually serves Lavalink v4.
        // A wrong proxy path's generic 404 must never release a claim.
        using var info = await GetAsync("/v4/info", cancellationToken);
        
        if (!info.IsSuccessStatusCode)
        {
            return false;
        }
        
        using var infoJson = await JsonDocument.ParseAsync(await info.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        
        if (!infoJson.RootElement.TryGetProperty("version", out var version) || !version.TryGetProperty("major", out var major) || major.GetInt32() != 4)
        {
            return false;
        }

        using var response = await GetAsync($"/v4/sessions/{Uri.EscapeDataString(sessionId)}/players", cancellationToken);
      
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            return false;
        }

        // Require a Lavalink error body for this exact resource.
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        
        return json.RootElement.TryGetProperty("status", out var status) &&
            status.GetInt32() == 404 &&
            json.RootElement.TryGetProperty("path", out var path) &&
            path.GetString() == $"/v4/sessions/{sessionId}/players";
    }

    public async Task<bool> IsPlayingAsync(string sessionId, ulong guildId, string encodedTrack, CancellationToken cancellationToken)
    {
        using var response = await GetAsync($"/v4/sessions/{Uri.EscapeDataString(sessionId)}/players/{guildId}", cancellationToken);
       
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = json.RootElement;
        
        return root.TryGetProperty("guildId", out var guild) &&
            guild.GetString() == guildId.ToString(System.Globalization.CultureInfo.InvariantCulture) &&
            root.TryGetProperty("track", out var track) &&
            track.ValueKind == JsonValueKind.Object &&
            track.TryGetProperty("encoded", out var encoded) &&
            encoded.GetString() == encodedTrack;
    }
}
