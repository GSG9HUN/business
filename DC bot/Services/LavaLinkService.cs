using DC_bot.Wrapper;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DC_bot.Services
{
    public class LavaLinkService
    {
        private readonly LavalinkExtension _lavalink;
        private readonly ILogger<LavaLinkService> _logger;

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


        public async Task PlayAsync(DiscordChannel voiceStateChannel, Uri url, DiscordChannel textChannel)
        {
            try
            {
                await ConnectAsync();
                var node = _lavalink.ConnectedNodes.Values.First();
                await node.ConnectAsync(voiceStateChannel);

                var searchQuery = await node.Rest.GetTracksAsync(url);

                if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
                {
                    await textChannel.SendMessageAsync($"Failed to find music with query: {url}");
                    _logger.LogInformation($"Failed to find music with query: {url}");
                    return;
                }

                var musicTrack = searchQuery.Tracks.First();

                await node.ConnectedGuilds.Values.First().PlayAsync(musicTrack);
                await textChannel.SendMessageAsync($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
                _logger.LogInformation($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw;
            }
        }

        public async Task PlayAsync(DiscordChannel voiceStateChannel, string query, DiscordChannel textChannel)
        {
            try
            {
                await ConnectAsync();
                var node = _lavalink.ConnectedNodes.Values.First();
                await node.ConnectAsync(voiceStateChannel);

                var searchQuery = await node.Rest.GetTracksAsync(query);

                if (searchQuery.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
                {
                    await textChannel.SendMessageAsync($"Failed to find music with query: {query}");
                    _logger.LogInformation($"Failed to find music with query: {query}");
                    return;
                }

                var musicTrack = searchQuery.Tracks.First();

                await node.ConnectedGuilds.Values.First().PlayAsync(musicTrack);
                await textChannel.SendMessageAsync($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
                _logger.LogInformation($"Music is playing : {musicTrack.Author} - {musicTrack.Title}");
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw;
            }
        }
    }
}