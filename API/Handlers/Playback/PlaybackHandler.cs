using API.Mapping;
using API.Requests.Playback;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playback;

public static class PlaybackHandlers
{
    public static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        IBotControlCommandsRepository repository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var commandName = (string)httpContext.Items["commandName"]!;

        var (userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            ct);
        if (accessError is not null)
        {
            return accessError;
        }
        
        var command = await repository.EnqueueAsync(
            guildId,
            userId,
            commandName,
            ct);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

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

        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!TryParseRepeatMode(request.Mode, out var repeatMode))
        {
            return HttpResults.BadRequest(new
            {
                ErrorMessage = "Repeat mode must be one of: none, one, all."
            });
        }

        switch (repeatMode)
        {
            case RepeatMode.None:
                await playbackStateRepository.SetRepeatStateAsync(
                    guildId,
                    isRepeating: false,
                    isRepeatingList: false,
                    cancellationToken);
                await repeatListRepository.ClearAsync(guildId, cancellationToken);
                break;

            case RepeatMode.One:
                await playbackStateRepository.SetRepeatStateAsync(
                    guildId,
                    isRepeating: true,
                    isRepeatingList: false,
                    cancellationToken);
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
                await playbackStateRepository.SetRepeatStateAsync(
                    guildId,
                    isRepeating: false,
                    isRepeatingList: true,
                    cancellationToken);
                break;
        }

        return HttpResults.NoContent();
    }

    private static bool TryParseRepeatMode(string? value, out RepeatMode repeatMode)
    {
        repeatMode = RepeatMode.None;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "none" => SetRepeatMode(out repeatMode, RepeatMode.None),
            "one" => SetRepeatMode(out repeatMode, RepeatMode.One),
            "all" => SetRepeatMode(out repeatMode, RepeatMode.All),
            _ => false
        };
    }

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
