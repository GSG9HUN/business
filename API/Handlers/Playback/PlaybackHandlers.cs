using API.Mapping;
using API.Requests.Playback;
using DC_bot.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playback;

/// <summary>
/// Accepts authorized playback commands and persists explicit repeat-mode changes.
/// </summary>
public static class PlaybackHandlers
{

    /// <summary>
    /// Persists an authorized playback command for asynchronous execution by the bot.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="repository">The injected persistence contract used by this operation.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="ct">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        IBotControlCommandsRepository repository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var commandName = (string)httpContext.Items["commandName"]!;
        var(userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, ct);

        if (accessError is not null)
        {
            return accessError;
        }

        var command = await repository.EnqueueAsync(guildId, userId, commandName, ct);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>
    /// Validates the requested repeat mode and persists repeat flags and the repeat-list snapshot.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="request">The client request to validate and process.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="playbackStateRepository">The injected persistence contract used by this operation.</param>
    /// <param name="queueRepository">The injected persistence contract used by this operation.</param>
    /// <param name="repeatListRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> SetRepeatModeAsync(
        HttpContext httpContext,
        SetRepeatModeRequest request,
        IMobileAppUserRepository userRepository,
        IPlaybackStateRepository playbackStateRepository,
        IQueueRepository queueRepository,
        IRepeatListRepository repeatListRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(_, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        if (!TryParseRepeatMode(request.Mode, out var repeatMode))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Repeat mode must be one of: none, one, all." });
        }

        switch (repeatMode)
        {
            case RepeatMode.None:
                await playbackStateRepository.SetRepeatStateAsync(guildId, isRepeating: false, isRepeatingList: false, MobileRealtimeEventNames.RepeatModeChanged, cancellationToken);
                await repeatListRepository.ClearAsync(guildId, cancellationToken);
                break;
            case RepeatMode.One:
                await playbackStateRepository.SetRepeatStateAsync(guildId, isRepeating: true, isRepeatingList: false, MobileRealtimeEventNames.RepeatModeChanged, cancellationToken);
                await repeatListRepository.ClearAsync(guildId, cancellationToken);
                break;
            case RepeatMode.All:
                var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
                var queuedItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
                var repeatList = new List<string>(queuedItems.Count + (state.CurrentTrackIdentifier is null ? 0 : 1));

                if (!string.IsNullOrWhiteSpace(state.CurrentTrackIdentifier))
                {
                    repeatList.Add(state.CurrentTrackIdentifier);
                }

                repeatList.AddRange(queuedItems.Select(item => item.TrackIdentifier));
                await repeatListRepository.ReplaceAsync(guildId, repeatList, cancellationToken);
                await playbackStateRepository.SetRepeatStateAsync(guildId, isRepeating: false, isRepeatingList: true, MobileRealtimeEventNames.RepeatModeChanged, cancellationToken);
                break;
        }

        return HttpResults.NoContent();
    }

    /// <summary>
    /// Recognizes the none, one, and all repeat-mode values without accepting unknown modes.
    /// </summary>
    /// <param name="value">The value value for this operation.</param>
    /// <param name="repeatMode">Receives the repeat mode result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    private static bool TryParseRepeatMode(string? value, out RepeatMode repeatMode)
    {
        repeatMode = RepeatMode.None;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim()
            .ToLowerInvariant() switch
        {
            "none" => SetRepeatMode(out repeatMode, RepeatMode.None),
            "one" => SetRepeatMode(out repeatMode, RepeatMode.One),
            "all" => SetRepeatMode(out repeatMode, RepeatMode.All),
            _ => false
        };
    }

    /// <summary>
    /// Assigns a recognized repeat mode to the parsing result.
    /// </summary>
    /// <param name="target">Receives the target result.</param>
    /// <param name="value">The value value for this operation.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    private static bool SetRepeatMode(out RepeatMode target, RepeatMode value)
    {
        target = value;
        return true;
    }

    private enum RepeatMode
    {
        None,
        One,
        All
    }
}
