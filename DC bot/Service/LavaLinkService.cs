using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class LavaLinkService(
    IMusicQueueService musicQueueService,
    ILogger<LavaLinkService> logger,
    IAudioService audioService,
    IValidationService validationService,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ILavaLinkService
{
    // event használata, hogy értesítsük a új zene kezdődik és hogy adjon hozzá emojikat.
    // TODO: A TrackStarted esemény null!-ra van inicializálva. Ha véletlenül nincs feliratkozó és az eseményt
    //       meghívják (Invoke), NullReferenceException keletkezik. Javasolt legalább egy null-ellenőrzést
    //       alkalmazni az Invoke előtt: TrackStarted?.Invoke(...).
    public event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted = null!;

    private readonly Dictionary<ulong, bool> _isPlaybackFinishedRegistered = new();
    public Dictionary<ulong, bool> IsRepeating { get; set; } = new();
    public Dictionary<ulong, bool> IsRepeatingList { get; set; } = new();

    private readonly Dictionary<ulong, LavalinkTrack?> _currentTrack = new();

    public void Init(ulong guildId)
    {
        _isPlaybackFinishedRegistered.Add(guildId, false);
        _currentTrack.Add(guildId, null);
        IsRepeating.Add(guildId, false);
        IsRepeatingList.Add(guildId, false);
    }

    public async Task ConnectAsync()
    {
        try
        {
            await audioService.StartAsync().ConfigureAwait(false);
            logger.LogInformation("Lavalink node connected successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lavalink connection failed: {Message}", ex.Message);
        }
    }

    // TODO: A PlayAsyncUrl és PlayAsyncQuery metódusok szinte teljesen azonos kódot tartalmaznak
    //       (validáció, eseményregisztráció, stb.). Érdemes lenne egy közös privát segédmetódusba kiemelni
    //       az ismétlődő logikát (pl. EnsureConnectedAndValidatedAsync) a DRY elvnek megfelelően.
    public async Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        await ConnectAsync();
        var textChannel = message.Channel;
        var guildId = textChannel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id, voiceStateChannel.Id)
            .ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (!_isPlaybackFinishedRegistered[guildId])
        {
            audioService.TrackEnded += async (_, args) =>
                await OnTrackFinished(connection, args, textChannel);
            _isPlaybackFinishedRegistered[guildId] = true;
            logger.LogInformation("PlaybackFinished event registered.");
        }

        var loadResult = await audioService.Tracks.LoadTracksAsync(url.ToString(), trackSearchMode)
            .ConfigureAwait(false);


        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await textChannel.SendMessageAsync(
                $"{localizationService.Get("play_command_failed_to_find_music_url_error")} {url}");
            logger.LogInformation("Failed to find music with url: {Url}", url);
            return;
        }

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }

    public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        await ConnectAsync();
        var textChannel = message.Channel;
        var guildId = textChannel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id, voiceStateChannel.Id)
            .ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (!_isPlaybackFinishedRegistered[guildId])
        {
            audioService.TrackEnded += async (_, args) =>
                await OnTrackFinished(connection, args, textChannel);
            _isPlaybackFinishedRegistered[guildId] = true;
            logger.LogInformation("PlaybackFinished event registered.");
        }

        var loadResult = await audioService.Tracks.LoadTracksAsync(query, trackSearchMode)
            .ConfigureAwait(false);


        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await textChannel.SendMessageAsync(
                $"{localizationService.Get("play_command_failed_to_find_music_url_error")} {query}");
            logger.LogInformation("Failed to find music with query: {Query}", query);
            return;
        }

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }

    public async Task PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await ConnectAsync();
        
        var channel = member?.VoiceState?.Channel;
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_a_voice_channel");
            return;
        }
        var guildId = channel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(channel.Guild.Id, channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (connection.CurrentTrack == null)
        {
            await channel.SendMessageAsync(localizationService.Get("pause_command_error"));
            logger.LogInformation("There is no track currently playing.");
            return;
        }

        await connection.PauseAsync();
        logger.LogInformation(
            $"{localizationService.Get("pause_command_response")} {connection.CurrentTrack.Title}");
    }

    public async Task ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await ConnectAsync();
        
        var channel = member?.VoiceState?.Channel;
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_a_voice_channel");
            return;
        }
        var guildId = channel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(channel.Guild.Id, channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (connection.CurrentTrack == null)
        {
            await channel.SendMessageAsync(localizationService.Get("resume_command_error"));
            logger.LogInformation("There is no track currently paused.");
            return;
        }

        await connection.ResumeAsync();
        logger.LogInformation(
            $"{localizationService.Get("resume_command_response")} {connection.CurrentTrack.Title}");
    }

    public async Task SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await ConnectAsync();
        
        var channel = member?.VoiceState?.Channel;
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_a_voice_channel");
            return;
        }
        var guildId = channel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(channel.Guild.Id, channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (connection.CurrentTrack == null && !musicQueueService.HasTracks(channel.Guild.Id))
        {
            await channel.SendMessageAsync(localizationService.Get("skip_command_error"));
            return;
        }

        await connection.StopAsync();
    }

    public IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId)
    {
        return musicQueueService.ViewQueue(guildId);
    }

    public string GetCurrentTrack(ulong guildId)
    {
        return _currentTrack[guildId]?.Author + " " + _currentTrack[guildId]?.Title;
    }

    public string GetCurrentTrackList(ulong guildId)
    {
        var response = _currentTrack[guildId]?.Author + " " + _currentTrack[guildId]?.Title + "\n";

        return musicQueueService.ViewQueue(guildId).Aggregate(response,
            (current, track) => current + (track.Author + " " + track.Title + "\n"));
    }

    public void CloneQueue(ulong guildId)
    {
        if (_currentTrack[guildId] == null) return;

        musicQueueService.Clone(guildId, _currentTrack[guildId]!);
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        await ConnectAsync();

        var voiceStateChannel = member?.VoiceState?.Channel;
        if (voiceStateChannel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_a_voice_channel");
            return;
        }
        var guildId = voiceStateChannel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id, voiceStateChannel.Id)
            .ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (validationConnectionResult.Connection != null)
            await validationConnectionResult.Connection.StopAsync();
        await connection.DisconnectAsync().ConfigureAwait(false);
    }

    public async Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel,
        IDiscordMember? member)
    {
        await ConnectAsync();
        
        var voiceStateChannel = member?.VoiceState?.Channel;
        if (voiceStateChannel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_a_voice_channel");
            return;
        }
        
        var guildId = voiceStateChannel.Guild.Id;

        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id, voiceStateChannel.Id)
            .ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
            .ConfigureAwait(false);

        if (!validationPlayerResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
            return;
        }

        var validationConnectionResult =
            await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

        if (!validationConnectionResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return;
        }

        if (!_isPlaybackFinishedRegistered[guildId])
        {
            audioService.TrackEnded += async (_, args) =>
                await OnTrackFinished(connection, args, textChannel);
            _isPlaybackFinishedRegistered[guildId] = true;
            logger.LogInformation("PlaybackFinished event registered.");
        }

        var nextTrack = musicQueueService.Dequeue(guildId);

        if (nextTrack is null)
        {
            return;
        }

        await connection.PlayAsync(nextTrack);

        await TrackStarted.Invoke(textChannel, SingletonDiscordClient.Instance,
            $"{localizationService.Get("play_command_music_playing")}{nextTrack.Author} - {nextTrack.Title}");

        _currentTrack[guildId] = nextTrack;
    }

    private async Task PlayTheFoundMusic(TrackLoadResult searchQuery, ILavalinkPlayer connection,
        IDiscordChannel textChannel)
    {
        var musicTrack = searchQuery.IsPlaylist ? searchQuery.Tracks.ToList() : [searchQuery.Track!];

        var guildId = textChannel.Guild.Id;

        musicTrack.ForEach(track => musicQueueService.Enqueue(guildId, new LavaLinkTrackWrapper(track)));

        if (connection.CurrentTrack == null)
        {
            var nextTrack = musicQueueService.Dequeue(guildId);

            if (nextTrack == null) return;

            await connection.PlayAsync(nextTrack);
            await TrackStarted.Invoke(textChannel, SingletonDiscordClient.Instance,
                $"{localizationService.Get("play_command_music_playing")}{nextTrack.Author} - {nextTrack.Title}");
            _currentTrack[guildId] = nextTrack;
            return;
        }

        if (musicTrack.Count > 1)
        {
            await textChannel.SendMessageAsync(localizationService.Get("play_command_list_added_queue"));
            logger.LogInformation("Added to queue.");
            return;
        }

        var track = musicTrack.First();
        await textChannel.SendMessageAsync(
            $"{localizationService.Get("play_command_music_added_queue")} {track.Author} - {track.Title}");
        logger.LogInformation("Added to queue: {Author} - {Title}", track.Author, track.Title);
    }
    
    private async Task OnTrackFinished(ILavalinkPlayer player, TrackEndedEventArgs args, IDiscordChannel textChannel)
    {
        if (!IsFinishedOrStopped(args.Reason))
            return;

        var guildId = textChannel.Guild.Id;

        if (TryRepeatCurrentTrack(guildId, out var repeatTrack))
        {
            if (repeatTrack == null)
            {   
                return;
            }

            await player.PlayAsync(repeatTrack);
            logger.LogInformation("Repeating: {RepeatTrackAuthor} - {RepeatTrackTitle}", repeatTrack.Author, repeatTrack.Title);
            return;
        }

        if (await TryPlayNextFromQueueAsync(player, textChannel, guildId))
            return;

        if (await TryRepeatListAndPlayAsync(player, textChannel, guildId))
            return;

        await NotifyQueueEmptyAsync(textChannel);
    }

    private static bool IsFinishedOrStopped(TrackEndReason reason)
        => reason is TrackEndReason.Finished or TrackEndReason.Stopped;

    private bool TryRepeatCurrentTrack(ulong guildId, out LavalinkTrack? track)
    {
        track = null;
        if (!IsRepeating.TryGetValue(guildId, out var repeating) || !repeating)
            return false;

        if (!_currentTrack.TryGetValue(guildId, out var current) || current is null)
            return false;

        track = current;
        return true;
    }

    private async Task<bool> TryPlayNextFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!musicQueueService.HasTracks(guildId))
            return false;

        await PlayTrackFromQueue(player, textChannel);
        return true;
    }

    private async Task<bool> TryRepeatListAndPlayAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!IsRepeatingList.TryGetValue(guildId, out var repeatList) || !repeatList)
            return false;

        if (musicQueueService.HasTracks(guildId))
            return false;

        foreach (var t in musicQueueService.GetRepeatableQueue(guildId))
            musicQueueService.Enqueue(guildId, t);

        await PlayTrackFromQueue(player, textChannel);
        return true;
    }

    private async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel)
    {
        await textChannel.SendMessageAsync(localizationService.Get("skip_command_queue_is_empty"));
        logger.LogInformation("Queue is empty. Playback has stopped.");
    }

    private async Task PlayTrackFromQueue(ILavalinkPlayer player, IDiscordChannel textChannel)
    {
        var guildId = textChannel.Guild.Id;
        var nextTrack = musicQueueService.Dequeue(guildId);

        if (nextTrack is null)
        {
            return;
        }

        await player.PlayAsync(nextTrack);

        await TrackStarted.Invoke(textChannel, SingletonDiscordClient.Instance,
            $"{localizationService.Get("skip_command_response")}{nextTrack.Author} - {nextTrack.Title}");
        logger.LogInformation("Now Playing: {Author} - {Title}", nextTrack.Author, nextTrack.Title);
    }

    internal async Task TrackStartedEventTrigger(IDiscordChannel channel, DiscordClient client, ILavaLinkTrack track)
    {
        await TrackStarted.Invoke(channel, client,
            $"{localizationService.Get("skip_command_response")}{track.Author} - {track.Title}");
    }
}