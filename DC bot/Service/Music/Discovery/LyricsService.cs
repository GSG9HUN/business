using System.Globalization;
using System.Net;
using System.Text.Json;
using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Service.Presentation;
using Lavalink4NET;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Discovery;

/// <summary>Retrieves paginated lyrics from the configured LavaLyrics plugin, without changing playback.</summary>
/// <param name="clients">Creates the configured authenticated Lavalink HTTP client.</param>
/// <param name="audio">Loads optional search seeds through existing source providers.</param>
/// <param name="resolver">Resolves the existing source search rules.</param>
/// <param name="sessions">Supplies the owned current track.</param>
/// <param name="localization">Formats command results in the guild language.</param>
/// <param name="logger">Records provider failures without including credentials or lyrics.</param>
public sealed class LyricsService(
    IHttpClientFactory clients,
    IAudioService audio,
    ITrackSearchResolverService resolver,
    GuildPlaybackSessions sessions,
    ILocalizationService localization,
    ILogger<LyricsService> logger) : ILyricsService
{

    /// <inheritdoc />
    public async Task<PlaybackControlResult> ExecuteAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;
        var query = arguments.Trim();
        var page = 1;
        var pageMarker = query.LastIndexOf("--page ", StringComparison.Ordinal);

        if (pageMarker >= 0)
        {
            if (!int.TryParse(query[(pageMarker + 7)..], NumberStyles.None, CultureInfo.InvariantCulture, out page)
                || page is < 1 or > 100)
            {
                return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
            }

            query = query[..pageMarker].Trim();
        }

        try
        {
            LavalinkTrack? track;

            if (query.Length == 0)
            {
                track = sessions.CaptureCurrent(guildId)?.Player.CurrentTrack;
            }
            else
            {
                var loaded = await audio.Tracks.LoadTracksAsync(query, resolver.ResolveSearchMode(query), cancellationToken: cancellationToken);
                track = loaded.IsPlaylist ? null : loaded.Track;
            }

            if (track is null)
            {
                return Fail(guildId, LocalizationKeys.DiscoveryNoTrack);
            }

            var playable = await track.GetPlayableTrackAsync(cancellationToken: cancellationToken);
            using var client = clients.CreateClient("MusicDiscovery");
            using var response = await client.GetAsync(
                "v4/lyrics?track=" + Uri.EscapeDataString(playable.ToString()), cancellationToken);

            if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
            {
                return Fail(guildId, LocalizationKeys.LyricsUnavailable);
            }

            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            var text = root.TryGetProperty("text", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() : null;

            if (string.IsNullOrWhiteSpace(text)
                && root.TryGetProperty("lines", out var lines)
                && lines.ValueKind == JsonValueKind.Array)
            {
                text = string.Join("\n", lines.EnumerateArray()
                    .Where(line => line.TryGetProperty("line", out var content) && content.ValueKind == JsonValueKind.String)
                    .Select(line => line.GetProperty("line").GetString()));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Fail(guildId, LocalizationKeys.LyricsUnavailable);
            }

            var snapshot = DiscordPaginationService.FromText(localization.Get(guildId, LocalizationKeys.ResponseLyricsTitle), text, page);
            var request = new DC_bot.Interface.Service.Presentation.Models.PaginationRequest(async (number, token) =>
            {
                var content = await snapshot.LoadPageAsync(number, token);
                return content with
                {
                    Description = localization.Get(guildId, LocalizationKeys.LyricsDesignBody,
                        DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(track.Title),
                        DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(track.Author),
                        content.Number, content.PageCount, content.Description)
                };
            }, page);
            var initial = await request.LoadPageAsync(page, cancellationToken);

            if (page > initial.PageCount)
            {
                return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
            }

            return PlaybackControlResult.Succeeded(localization.Get(guildId, LocalizationKeys.LyricsPage,
                track.Title, initial.Number, initial.PageCount, initial.Description)) with
            {
                Pagination = request with { InitialContent = initial }
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Lyrics lookup failed. Guild={GuildId}", guildId);
            return Fail(guildId, LocalizationKeys.LyricsUnavailable);
        }
    }

    /// <summary>Creates a localized unavailable or validation result.</summary>
    private PlaybackControlResult Fail(ulong guildId, string key) =>
        PlaybackControlResult.Failed(localization.Get(guildId, key), key);
}
