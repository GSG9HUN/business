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
    ILocalizationService localizationService) : ILavaLinkService
{
    // event használata, hogy értesítsük a új zene kezdődik és hogy adjon hozzá emojikat.
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
            logger.LogError($"Lavalink connection failed: {ex.Message}");
        }
    }

    public async Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordChannel textChannel)
    {
        await ConnectAsync();
        var guildId = textChannel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id,voiceStateChannel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, voiceStateChannel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, voiceStateChannel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
            return;
        }

        if (!_isPlaybackFinishedRegistered[guildId])
        {
            audioService.TrackEnded += async (_, args) =>
                await OnTrackFinished(connection, args, textChannel);
            _isPlaybackFinishedRegistered[guildId] = true;
            logger.LogInformation("PlaybackFinished event registered.");
        }

        var loadResult = await audioService.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube)
            .ConfigureAwait(false);


        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await textChannel.SendMessageAsync(
                $"{localizationService.Get("play_command_failed_to_find_music_url_error")} {url}");
            logger.LogInformation($"Failed to find music with url: {url}");
            return;
        }

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }

    public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordChannel textChannel)
    {
        await ConnectAsync();
        var guildId = textChannel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id,voiceStateChannel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, voiceStateChannel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, voiceStateChannel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
            return;
        }

        if (!_isPlaybackFinishedRegistered[guildId])
        {
            audioService.TrackEnded += async (_, args) =>
                await OnTrackFinished(connection, args, textChannel);
            _isPlaybackFinishedRegistered[guildId] = true;
            logger.LogInformation("PlaybackFinished event registered.");
        }

        var loadResult = await audioService.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);


        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await textChannel.SendMessageAsync(
                $"{localizationService.Get("play_command_failed_to_find_music_url_error")} {query}");
            logger.LogInformation($"Failed to find music with url: {query}");
            return;
        }

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }
    
    public async Task PauseAsync(IDiscordChannel channel)
    {
        await ConnectAsync();
        var guildId = channel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(channel.Guild.Id,channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, channel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, channel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
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

    public async Task ResumeAsync(IDiscordChannel channel)
    {
        await ConnectAsync();
        var guildId = channel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(channel.Guild.Id,channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, channel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, channel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
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

    public async Task SkipAsync(IDiscordChannel channel)
    {
        await ConnectAsync();
        var guildId = channel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(channel.Guild.Id,channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, channel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, channel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
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

    public async Task LeaveVoiceChannel(IDiscordChannel channel)
    {
        await ConnectAsync();
        var guildId = channel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(channel.Guild.Id,channel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, channel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, channel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid)
        {
            return;
        }

        await connection.DisconnectAsync().ConfigureAwait(false);
    }

    public async Task StartPlayingQueue(IDiscordChannel voiceStateChannel, IDiscordChannel textChannel)
    {
        await ConnectAsync();
        var guildId = voiceStateChannel.Guild.Id;
        
        var connection = await audioService.Players.JoinAsync(voiceStateChannel.Guild.Id,voiceStateChannel.Id).ConfigureAwait(false);

        var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId, voiceStateChannel)
            .ConfigureAwait(false);
        
        if (!validationPlayerResult.isValid)
        {
            return;
        }
        
        var validationConnectionResult = await validationService.ValidateConnectionAsync(connection, voiceStateChannel).ConfigureAwait(false);
        
        if (!validationConnectionResult.isValid || musicQueueService.HasTracks(guildId))
        {
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
            $"{localizationService.Get("play_command_music_playing")} {nextTrack.Author} - {nextTrack.Title}");

        _currentTrack[guildId] = nextTrack;
    }

    private async Task PlayTheFoundMusic(TrackLoadResult searchQuery, ILavalinkPlayer connection,
        IDiscordChannel textChannel)
    {
        var musicTrack = searchQuery.IsPlaylist ? searchQuery.Tracks.ToList() : [searchQuery.Track];
        
        var guildId = textChannel.Guild.Id;

        musicTrack.ForEach(track => musicQueueService.Enqueue(guildId, new LavaLinkTrackWrapper(track)));

        if (connection.CurrentTrack == null)
        {
            var nextTrack = musicQueueService.Dequeue(guildId);

            if (nextTrack == null) return;

            await connection.PlayAsync(nextTrack);
            await TrackStarted.Invoke(textChannel, SingletonDiscordClient.Instance,
                $"{localizationService.Get("play_command_music_playing")} {nextTrack.Author} - {nextTrack.Title}");
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
        logger.LogInformation($"Added to queue: {track.Author} - {track.Title}");
    }

    private async Task OnTrackFinished(ILavalinkPlayer player, TrackEndedEventArgs args, IDiscordChannel textChannel)
    {
        var finishedOrStopped = args.Reason is TrackEndReason.Finished or TrackEndReason.Stopped;
        var guildId = textChannel.Guild.Id;

        switch (finishedOrStopped)
        {
            case true when IsRepeating[guildId] && _currentTrack.TryGetValue(guildId, out var track) &&
                           track is not null:
                await player.PlayAsync(track);
                logger.LogInformation(
                    $"Repeating: {_currentTrack[guildId]?.Author} - {_currentTrack[guildId]?.Title}");
                return;
            case true when musicQueueService.HasTracks(guildId):
                await PlayTrackFromQueue(player, textChannel);
                return;
            case true when !musicQueueService.HasTracks(guildId) && IsRepeatingList[guildId]:
            {
                var repeatableQueue = musicQueueService.GetRepeatableQueue(guildId);
                foreach (var track in repeatableQueue)
                {
                    musicQueueService.Enqueue(guildId, track);
                }

                await PlayTrackFromQueue(player, textChannel);
                return;
            }
            default:
                await textChannel.SendMessageAsync(localizationService.Get("skip_command_queue_is_empty"));
                logger.LogInformation("Queue is empty. Playback has stopped.");
                break;
        }
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
            $"{localizationService.Get("skip_command_response")} {nextTrack?.Author} - {nextTrack?.Title}");
        logger.LogInformation($"Now Playing: {nextTrack?.Author} - {nextTrack?.Title}");
    }
}