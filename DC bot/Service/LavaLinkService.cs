using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service
{
    public class LavaLinkService(
        IMusicQueueService musicQueueService,
        ILogger<LavaLinkService> logger,
        ILocalizationService localizationService) : ILavaLinkService
    {
        // event használata, hogy értesítsük a új zene kezdődik és hogy adjon hozzá emojikat.
        public event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted = null!;

        private readonly LavalinkExtension _lavalink = SingletonDiscordClient.Instance.GetLavalink();

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
            if (_lavalink.ConnectedNodes.Count > 0)
            {
                logger.LogInformation("Lavalink is already connected");
                return;
            }

            try
            {
                var endpoint = new ConnectionEndpoint
                {
                    Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOSTNAME"),
                    Port = int.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT") ?? "2333"),
                    Secured = bool.Parse(Environment.GetEnvironmentVariable("LAVALINK_SECURED") ?? "false")
                };

                var lavalinkConfig = new LavalinkConfiguration
                {
                    Password = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD"),
                    RestEndpoint = endpoint,
                    SocketEndpoint = endpoint
                };

                await _lavalink.ConnectAsync(lavalinkConfig);
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
            var node = _lavalink.ConnectedNodes.Values.FirstOrDefault();

            if (node == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("lavalink_error"));
                logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = await node.ConnectAsync(voiceStateChannel.ToDiscordChannel());

            if (connection == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("bot_is_not_connected_error"));
                logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            var guildId = textChannel.Guild.Id;

            if (!_isPlaybackFinishedRegistered[guildId])
            {
                connection.PlaybackFinished += async (conn, args) =>
                    await OnTrackFinished(conn, args, textChannel);
                _isPlaybackFinishedRegistered[guildId] = true;
                logger.LogInformation("PlaybackFinished event registered.");
            }

            var searchQuery = await node.Rest.GetTracksAsync(url);

            if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
            {
                await textChannel.SendMessageAsync(
                    $"{localizationService.Get("play_command_failed_to_find_music_url_error")} {url}");
                logger.LogInformation($"Failed to find music with url: {url}");
                return;
            }

            await PlayTheFoundMusic(searchQuery, connection, textChannel);
        }

        public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordChannel textChannel)
        {
            await ConnectAsync();
            var node = _lavalink.ConnectedNodes.Values.FirstOrDefault();

            if (node == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("lavalink_error"));
                logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = await node.ConnectAsync(voiceStateChannel.ToDiscordChannel());

            if (connection == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("bot_is_not_connected_error"));
                logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            var guildId = textChannel.Guild.Id;

            if (!_isPlaybackFinishedRegistered[guildId])
            {
                connection.PlaybackFinished += async (conn, args) =>
                    await OnTrackFinished(conn, args, textChannel);
                _isPlaybackFinishedRegistered[guildId] = true;
                logger.LogInformation("PlaybackFinished event registered.");
            }

            var searchQuery = await node.Rest.GetTracksAsync(query);

            if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
            {
                await textChannel.SendMessageAsync(
                    $"{localizationService.Get("play_command_failed_to_find_music_query_error")} {query}");
                logger.LogInformation($"Failed to find music with query: {query}");
                return;
            }

            await PlayTheFoundMusic(searchQuery, connection, textChannel);
        }

        public async Task PauseAsync(IDiscordChannel channel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await channel.SendMessageAsync(localizationService.Get("lavalink_error"));
                logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(channel.Guild.ToDiscordGuild());

            if (connection == null)
            {
                await channel.SendMessageAsync(localizationService.Get("bot_is_not_connected_error"));
                logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                await channel.SendMessageAsync(localizationService.Get("pause_command_error"));
                logger.LogInformation("There is no track currently playing.");
                return;
            }

            await connection.PauseAsync();
            logger.LogInformation(
                $"{localizationService.Get("pause_command_response")} {connection.CurrentState.CurrentTrack.Title}");
        }

        public async Task ResumeAsync(IDiscordChannel textChannel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("lavalink_error"));
                logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(textChannel.Guild.ToDiscordGuild());

            if (connection == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("bot_is_not_connected_error"));
                logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("resume_command_error"));
                logger.LogInformation("There is no track currently paused.");
                return;
            }

            await connection.ResumeAsync();
            logger.LogInformation(
                $"{localizationService.Get("resume_command_response")} {connection.CurrentState.CurrentTrack.Title}");
        }

        public async Task SkipAsync(IDiscordChannel textChannel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("lavalink_error"));
                logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(textChannel.Guild.ToDiscordGuild());

            if (connection == null)
            {
                await textChannel.SendMessageAsync(localizationService.Get("bot_is_not_connected_error"));
                logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null && !musicQueueService.HasTracks(textChannel.Guild.Id))
            {
                await textChannel.SendMessageAsync(localizationService.Get("skip_command_error"));
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

        private async Task PlayTheFoundMusic(LavalinkLoadResult searchQuery, LavalinkGuildConnection connection,
            IDiscordChannel textChannel)
        {
            var musicTrack = searchQuery.Tracks.ToList();
            var guildId = textChannel.Guild.Id;

            musicTrack.ForEach(track => musicQueueService.Enqueue(guildId, new LavaLinkTrackWrapper(track)));

            if (connection.CurrentState.CurrentTrack == null)
            {
                var nextTrack = musicQueueService.Dequeue(guildId);

                if (nextTrack == null) return;

                await connection.PlayAsync(nextTrack);
                await TrackStarted.Invoke(textChannel, connection.Node.Discord,
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

        private async Task OnTrackFinished(LavalinkGuildConnection connection,
            TrackFinishEventArgs args, IDiscordChannel textChannel)
        {
            var finishedOrStopped = args.Reason is TrackEndReason.Finished or TrackEndReason.Stopped;
            var guildId = textChannel.Guild.Id;

            switch (finishedOrStopped)
            {
                case true when IsRepeating[guildId] && _currentTrack[guildId] != null:
                    await connection.PlayAsync(_currentTrack[guildId]);
                    logger.LogInformation(
                        $"Repeating: {_currentTrack[guildId]?.Author} - {_currentTrack[guildId]?.Title}");
                    return;
                case true when musicQueueService.HasTracks(guildId):
                    await PlayTrackFromQueue(connection, textChannel);
                    return;
                case true when !musicQueueService.HasTracks(guildId) && IsRepeatingList[guildId]:
                {
                    var repeatableQueue = musicQueueService.GetRepeatableQueue(guildId);
                    foreach (var track in repeatableQueue)
                    {
                        musicQueueService.Enqueue(guildId, track);
                    }

                    await PlayTrackFromQueue(connection, textChannel);
                    return;
                }
                default:
                    await textChannel.SendMessageAsync(localizationService.Get("skip_command_queue_is_empty"));
                    logger.LogInformation("Queue is empty. Playback has stopped.");
                    break;
            }
        }

        private async Task PlayTrackFromQueue(LavalinkGuildConnection connection, IDiscordChannel textChannel)
        {
            var guildId = textChannel.Guild.Id;
            var nextTrack = musicQueueService.Dequeue(guildId);

            await connection.PlayAsync(nextTrack);

            await TrackStarted.Invoke(textChannel, connection.Node.Discord,
                $"{localizationService.Get("skip_command_response")} {nextTrack?.Author} - {nextTrack?.Title}");
            logger.LogInformation($"Now Playing: {nextTrack?.Author} - {nextTrack?.Title}");
        }
    }
}