using DC_bot.Wrapper;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DC_bot.Services
{
    public class LavaLinkService
    {
        private readonly LavalinkExtension _lavalink;
        private readonly ILogger<LavaLinkService> _logger;
        private readonly MusicQueueService _musicQueueService = new();
        private bool _isPlaybackFinishedRegistered = false;
        private LavalinkTrack _currentTrack;
        private bool _isTrackProcessing = false;
        public bool IsRepeating { get; set; }

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
                    Port = int.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT")),
                    Secured = bool.Parse(Environment.GetEnvironmentVariable("LAVALINK_SECURED"))
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

        public async Task PlayAsyncURL(DiscordChannel voiceStateChannel, Uri url, DiscordChannel textChannel)
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

        private async Task PlayTheFoundMusic(LavalinkLoadResult searchQuery, LavalinkGuildConnection connection,
            DiscordChannel textChannel)
        {
            var musicTrack = searchQuery.Tracks.First();

            if (connection.CurrentState.CurrentTrack == null)
            {
                await connection.PlayAsync(musicTrack);
                await textChannel.SendMessageAsync($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
                _logger.LogInformation($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
                _currentTrack = musicTrack;
            }
            else
            {
                _musicQueueService.Enqueue(musicTrack);
                await textChannel.SendMessageAsync($"Added to queue: {musicTrack.Author} - {musicTrack.Title}");
                _logger.LogInformation($"Added to queue: {musicTrack.Author} - {musicTrack.Title}");
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
            await textChannel.SendMessageAsync($"Paused: {connection.CurrentState.CurrentTrack.Title}");
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
            await textChannel.SendMessageAsync($"Resumed: {connection.CurrentState.CurrentTrack.Title}");
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
            await textChannel.SendMessageAsync("Skipped to the next track.");
        }

        private async Task OnTrackFinished(LavalinkGuildConnection connection,
            TrackFinishEventArgs args, DiscordChannel textChannel)
        { 
            var finishedOrStopped = args.Reason is TrackEndReason.Finished or TrackEndReason.Stopped;
            
            if (finishedOrStopped && IsRepeating)
            {
                await connection.PlayAsync(_currentTrack);
                await textChannel.SendMessageAsync($"Repeating: {_currentTrack.Author} - {_currentTrack.Title}");
                _logger.LogInformation($"Repeating: {_currentTrack.Author} - {_currentTrack.Title}");
                return;
            }

            if (finishedOrStopped && _musicQueueService.HasTracks)
            {
                var nextTrack = _musicQueueService.Dequeue();
                await connection.PlayAsync(nextTrack);
                await textChannel.SendMessageAsync($"Now Playing: {nextTrack.Author} - {nextTrack.Title}");
                _logger.LogInformation($"Now Playing: {nextTrack.Author} - {nextTrack.Title}");
            }
            else
            {
                await textChannel.SendMessageAsync("Queue is empty. Playback has stopped.");
                _logger.LogInformation($"Queue is empty. Playback has stopped.");
            }
        }

        public IReadOnlyCollection<LavalinkTrack> ViewQueue()
        {
            return _musicQueueService.ViewQueue();
        }

        public string GetCurrentTrack()
        {
            return _currentTrack.Author + " " + _currentTrack.Title;
        }
    }
}