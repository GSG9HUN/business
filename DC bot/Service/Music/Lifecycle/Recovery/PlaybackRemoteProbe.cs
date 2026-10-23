using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Lavalink4NET;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.Recovery;

/// <summary>
/// Queries Lavalink REST state to confirm voice identity, pending starts, and expired node sessions.
/// </summary>
/// <param name="httpClientFactory">The http client factory dependency used by this boundary.</param>
/// <param name="nodeOptions">The configured typed options consumed by this boundary.</param>
public sealed class PlaybackRemoteProbe(IHttpClientFactory httpClientFactory, IOptions<AudioServiceOptions> nodeOptions)
{

    /// <summary>
    /// The Lavalink node base address derived from the configured host, port, and TLS setting.
    /// </summary>
    public string NodeAddress => nodeOptions.Value.BaseAddress.GetLeftPart(UriPartial.Path)
        .TrimEnd('/');

    /// <summary>
    /// Checks remote connection state against guild, Discord voice session, and available channel identity.
    /// </summary>
    /// <param name="nodeSessionId">The Lavalink node-session identity associated with the player.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="discordVoiceSessionId">The Discord voice-session identity expected in remote player state.</param>
    /// <param name="voiceChannelId">The voice channel id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> IsVoiceConnectedAsync(
        string nodeSessionId,
        ulong guildId,
        string discordVoiceSessionId,
        ulong voiceChannelId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(nodeSessionId)
            || string.IsNullOrWhiteSpace(discordVoiceSessionId)
            || voiceChannelId == 0)
        {
            return false;
        }

        using var response = await GetAsync($"/v4/sessions/{Uri.EscapeDataString(nodeSessionId)}/players/{guildId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        if (!root.TryGetProperty("guildId", out var guild)
            || guild.GetString() != guildId.ToString(System.Globalization.CultureInfo.InvariantCulture)
            || !root.TryGetProperty("state", out var state)
            || !state.TryGetProperty("connected", out var connected)
            || connected.ValueKind != JsonValueKind.True
            || !root.TryGetProperty("voice", out var voice)
            || !voice.TryGetProperty("sessionId", out var session)
            || session.GetString() != discordVoiceSessionId)
        {
            return false;
        }

        // Older responses may omit channelId. If present, it must match.
        if (voice.TryGetProperty("channelId", out var channel))
        {
            return channel.ValueKind == JsonValueKind.String && channel.GetString() == voiceChannelId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return true;
    }

    /// <summary>
    /// Sends an authenticated Lavalink REST request using the configured named client.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<HttpResponseMessage> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var client = httpClientFactory.CreateClient("PlaybackRecovery");
        using var request = new HttpRequestMessage(HttpMethod.Get, NodeAddress + path);
        request.Headers.TryAddWithoutValidation("Authorization", nodeOptions.Value.Passphrase);
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    /// <summary>
    /// Checks remote session evidence before treating an old node session as absent.
    /// </summary>
    /// <param name="nodeAddress">The node address supplied for this operation.</param>
    /// <param name="sessionId">The session id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> IsSessionGoneAsync(string nodeAddress, string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        if (!infoJson.RootElement.TryGetProperty("version", out var version)
            || !version.TryGetProperty("major", out var major)
            || major.GetInt32() != 4)
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
        return json.RootElement.TryGetProperty("status", out var status) && status.GetInt32() == 404 && json.RootElement.TryGetProperty("path", out var path) && path.GetString() == $"/v4/sessions/{sessionId}/players";
    }

    /// <summary>
    /// Checks whether the current guild player has active track playback.
    /// </summary>
    /// <param name="sessionId">The session id supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="encodedTrack">The encoded track supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> IsPlayingAsync(
        string sessionId,
        ulong guildId,
        string encodedTrack,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var response = await GetAsync($"/v4/sessions/{Uri.EscapeDataString(sessionId)}/players/{guildId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = json.RootElement;
        return root.TryGetProperty("guildId", out var guild) && guild.GetString() == guildId.ToString(System.Globalization.CultureInfo.InvariantCulture) && root.TryGetProperty("track", out var track) && track.ValueKind == JsonValueKind.Object && track.TryGetProperty("encoded", out var encoded) && encoded.GetString() == encodedTrack;
    }

    /// <summary>
    /// Confirms that the identified node session no longer contains the guild player.
    /// </summary>
    /// <param name="nodeSessionId">The old Lavalink session being checked after owned transports have drained.</param>
    /// <param name="guildId">The guild whose player must be absent from that session.</param>
    /// <param name="cancellationToken">Cancels authenticated evidence requests.</param>
    /// <returns>True only for a validated missing session or a valid player list excluding this guild.</returns>
    /// <remarks>Registry absence alone is not evidence; do not use this snapshot to release an in-flight unknown start.</remarks>
    public async Task<bool> IsPlayerGoneAsync(
        string nodeSessionId,
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeSessionId))
        {
            return false;
        }

        if (await IsSessionGoneAsync(
                NodeAddress,
                nodeSessionId,
                cancellationToken))
        {
            return true;
        }

        using var response = await GetAsync(
            $"/v4/sessions/{Uri.EscapeDataString(nodeSessionId)}/players",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        using var json = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        if (json.RootElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var expectedGuildId = guildId.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        foreach (var player in json.RootElement.EnumerateArray())
        {
            if (!player.TryGetProperty("guildId", out var id)
                || id.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            if (id.GetString() == expectedGuildId)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Restores a track and its position in a paused state using one remote update.
    /// </summary>
    /// <param name="nodeSessionId">The current node session already bound to the owned start claim.</param>
    /// <param name="guildId">The guild whose registered player receives the track.</param>
    /// <param name="encodedTrack">The playable encoded track identity bound to the pending attempt.</param>
    /// <param name="position">The nonnegative source-track position to restore.</param>
    /// <param name="cancellationToken">Cancels supported HTTP work without proving a remote rollback.</param>
    /// <returns>A task that completes when Lavalink acknowledges the paused update.</returns>
    /// <remarks>Invoke inside the existing start transport fence; a failed request may still have remote effects.</remarks>
    public async Task StartPausedAsync(
        string nodeSessionId,
        ulong guildId,
        string encodedTrack,
        TimeSpan position,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeSessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedTrack);

        if (position < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        var client = httpClientFactory.CreateClient("PlaybackRecovery");

        var path =
            $"/v4/sessions/{Uri.EscapeDataString(nodeSessionId)}/players/{guildId}";

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            NodeAddress + path);

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            nodeOptions.Value.Passphrase);

        request.Content = JsonContent.Create(new
        {
            track = new
            {
                encoded = encodedTrack
            },
            position = checked((long)position.TotalMilliseconds),
            paused = true
        });

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
