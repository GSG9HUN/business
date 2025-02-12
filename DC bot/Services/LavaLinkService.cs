using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DC_bot.Services
{
    public class LavaLinkService
    {
        // event használata, hogy értesítsük a új zene kezdődik és hogy adjon hozzá emojikat.
        public event Func<DiscordChannel, DiscordClient, string, Task> TrackStarted;
        
        private readonly LavalinkExtension _lavalink;
        private readonly ILogger<LavaLinkService> _logger;
        private readonly MusicQueueService _musicQueueService = new();
        private bool _isPlaybackFinishedRegistered = false;
        private LavalinkTrack? _currentTrack;
        public bool IsRepeating { get; set; }
        public bool IsRepeatingList { get; set; }

        public LavaLinkService(ILogger<LavaLinkService> logger)
        {
            _lavalink = SingletonDiscordClient.Instance.GetLavalink();
            _logger = logger;
        }

        public async Task ConnectAsync()
        {
            if (_lavalink == null)
            {
                _logger.LogError("Lavalink is not initialized");
                return;
            }

            if (_lavalink.ConnectedNodes.Count > 0)
            {
                _logger.LogInformation("Lavalink is already connected");
                return;
            }

            try
            {
                var endpoint = new ConnectionEndpoint
                {
                    Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOSTNAME"),
                    Port = int.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT") ?? throw new InvalidOperationException()),
                    Secured = bool.Parse(Environment.GetEnvironmentVariable("LAVALINK_SECURED") ?? throw new InvalidOperationException())
                };

                var lavalinkConfig = new LavalinkConfiguration
                {
                    Password = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD"),
                    RestEndpoint = endpoint,
                    SocketEndpoint = endpoint
                };

                await _lavalink.ConnectAsync(lavalinkConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return;
            }

            _logger.LogInformation("Lavalink node connected successfully");
        }

        public async Task PlayAsyncUrl(DiscordChannel voiceStateChannel, Uri url, DiscordChannel textChannel)
        {
            try
            {
                await ConnectAsync();
                var node = _lavalink.ConnectedNodes.Values.First();

                if (node == null)
                {
                    await textChannel.SendMessageAsync("Lavalink is not connected.");
                    _logger.LogInformation("Lavalink is not connected.");
                    return;
                }

                var connection = await node.ConnectAsync(voiceStateChannel);

                if (connection == null)
                {
                    await textChannel.SendMessageAsync("Bot is not connected to a voice channel.");
                    _logger.LogInformation("Bot is not connected to a voice channel.");
                    return;
                }

                if (!_isPlaybackFinishedRegistered)
                {
                    connection.PlaybackFinished += async (conn, args) =>
                        await OnTrackFinished(conn, args, textChannel);
                    _isPlaybackFinishedRegistered = true;
                    _logger.LogInformation("PlaybackFinished event registered.");
                }

                var searchQuery = await node.Rest.GetTracksAsync(url);

                if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
                {
                    await textChannel.SendMessageAsync($"Failed to find music with query: {url}");
                    _logger.LogInformation($"Failed to find music with query: {url}");
                    return;
                }

                await PlayTheFoundMusic(searchQuery, connection, textChannel);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw;
            }
        }

        public async Task PlayAsyncQuery(DiscordChannel voiceStateChannel, string query, DiscordChannel textChannel)
        {
            try
            {
                await ConnectAsync();
                var node = _lavalink.ConnectedNodes.Values.First();

                if (node == null)
                {
                    await textChannel.SendMessageAsync("Lavalink is not connected.");
                    _logger.LogInformation("Lavalink is not connected.");
                    return;
                }

                var connection = await node.ConnectAsync(voiceStateChannel);

                if (connection == null)
                {
                    await textChannel.SendMessageAsync("Bot is not connected to a voice channel.");
                    _logger.LogInformation("Bot is not connected to a voice channel.");
                    return;
                }

                if (!_isPlaybackFinishedRegistered)
                {
                    connection.PlaybackFinished += async (conn, args) =>
                        await OnTrackFinished(conn, args, textChannel);
                    _isPlaybackFinishedRegistered = true;
                    _logger.LogInformation("PlaybackFinished event registered.");
                }

                var searchQuery = await node.Rest.GetTracksAsync(query);

                if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
                {
                    await textChannel.SendMessageAsync($"Failed to find music with query: {query}");
                    _logger.LogInformation($"Failed to find music with query: {query}");
                    return;
                }

                await PlayTheFoundMusic(searchQuery, connection, textChannel);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw;
            }
        }

        public async Task PauseAsync(DiscordChannel textChannel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await textChannel.SendMessageAsync("Lavalink is not connected.");
                _logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(textChannel.Guild);

            if (connection == null)
            {
                await textChannel.SendMessageAsync("Bot is not connected to a voice channel.");
                _logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                await textChannel.SendMessageAsync("There is no track currently playing.");
                _logger.LogInformation("There is no track currently playing.");
                return;
            }

            await connection.PauseAsync();
            _logger.LogInformation($"Paused: {connection.CurrentState.CurrentTrack.Title}");
        }

        public async Task ResumeAsync(DiscordChannel textChannel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await textChannel.SendMessageAsync("Lavalink is not connected.");
                _logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(textChannel.Guild);

            if (connection == null)
            {
                await textChannel.SendMessageAsync("Bot is not connected to a voice channel.");
                _logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                await textChannel.SendMessageAsync("There is no track currently paused.");
                _logger.LogInformation("There is no track currently paused.");
                return;
            }

            await connection.ResumeAsync();
            _logger.LogInformation($"Resumed: {connection.CurrentState.CurrentTrack.Title}");
        }

        public async Task SkipAsync(DiscordChannel textChannel)
        {
            var node = _lavalink.ConnectedNodes.Values.First();

            if (node == null)
            {
                await textChannel.SendMessageAsync("Lavalink is not connected.");
                _logger.LogInformation("Lavalink is not connected.");
                return;
            }

            var connection = node.GetGuildConnection(textChannel.Guild);

            if (connection == null)
            {
                await textChannel.SendMessageAsync("Bot is not connected to a voice channel.");
                _logger.LogInformation("Bot is not connected to a voice channel.");
                return;
            }

            if (connection?.CurrentState.CurrentTrack == null)
            {
                await textChannel.SendMessageAsync("No track is currently playing.");
                return;
            }

            await connection.StopAsync();
        }

        public IReadOnlyCollection<LavalinkTrack> ViewQueue()
        {
            return _musicQueueService.ViewQueue();
        }

        public string GetCurrentTrack()
        {
            return _currentTrack.Author + " " + _currentTrack.Title;
        }

        public string GetCurrentTrackList()
        {
            var response = _currentTrack.Author + " " + _currentTrack.Title + "\n";
            foreach (var track in _musicQueueService.ViewQueue())
            {
                response += track.Author + " " + track.Title + "\n";
            }

            return response;
        }

        public void CloneQueue()
        {
            _musicQueueService.Clone(_currentTrack);
        }

        private async Task PlayTheFoundMusic(LavalinkLoadResult searchQuery, LavalinkGuildConnection connection,
            DiscordChannel textChannel)
        {
            var musicTrack = searchQuery.Tracks.First();

            if (connection.CurrentState.CurrentTrack == null)
            {
                await connection.PlayAsync(musicTrack);
                await TrackStarted.Invoke(textChannel, connection.Node.Discord,
                    $"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
                _currentTrack = musicTrack;
            }
            else
            {
                _musicQueueService.Enqueue(musicTrack);
                await textChannel.SendMessageAsync($"Added to queue: {musicTrack.Author} - {musicTrack.Title}");
                _logger.LogInformation($"Added to queue: {musicTrack.Author} - {musicTrack.Title}");
            }
        }

        private async Task OnTrackFinished(LavalinkGuildConnection connection,
            TrackFinishEventArgs args, DiscordChannel textChannel)
        {
            var finishedOrStopped = args.Reason is TrackEndReason.Finished or TrackEndReason.Stopped;

            switch (finishedOrStopped)
            {
                case true when IsRepeating:
                    await connection.PlayAsync(_currentTrack);
                    _logger.LogInformation($"Repeating: {_currentTrack.Author} - {_currentTrack.Title}");
                    return;
                case true when _musicQueueService.HasTracks:
                    PlayTrackFromQueue(connection, textChannel);
                    return;
                case true when !_musicQueueService.HasTracks && IsRepeatingList:
                {
                    foreach (var track in _musicQueueService.repeatableQueue)
                    {
                        _musicQueueService.Enqueue(track);
                    }

                    PlayTrackFromQueue(connection, textChannel);
                    return;
                }
                default:
                    await textChannel.SendMessageAsync("Queue is empty. Playback has stopped.");
                    _logger.LogInformation($"Queue is empty. Playback has stopped.");
                    break;
            }
        }

        private async void PlayTrackFromQueue(LavalinkGuildConnection connection, DiscordChannel textChannel)
        {
            var nextTrack = _musicQueueService.Dequeue();
            await connection.PlayAsync(nextTrack);
            await textChannel.SendMessageAsync($"Now Playing: {nextTrack?.Author} - {nextTrack?.Title}");
            _logger.LogInformation($"Now Playing: {nextTrack?.Author} - {nextTrack?.Title}");
        }
    }
}