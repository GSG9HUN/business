using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.MusicServices;

public class CurrentTrackService(
    IPlaybackStateRepository playbackStateRepository,
    ILogger<CurrentTrackService>? logger = null) : ICurrentTrackService
{
    private readonly ILogger<CurrentTrackService> _logger = logger ?? NullLogger<CurrentTrackService>.Instance;

    public async Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
        if (state.CurrentTrackIdentifier is null)
            return null;

        try
        {
            var track = LavalinkTrack.Parse(state.CurrentTrackIdentifier, null);
            return new LavaLinkTrackWrapper(track)
            {
                QueueItemId = state.QueueItemId
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse current track identifier for guild {GuildId}.", guildId);
            return null;
        }
    }

    public async Task SetCurrentTrackAsync(ulong guildId, ILavaLinkTrack? track, CancellationToken cancellationToken = default)
    {
        var identifier = track?.ToString();
    
        long? queueItemId = null;
        if (track is LavaLinkTrackWrapper wrapper)
        {
            queueItemId = wrapper.QueueItemId;
        }
        
        await playbackStateRepository.SetCurrentTrackAsync(guildId, identifier, queueItemId, cancellationToken);
    }

    public async Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var track = await GetCurrentTrackAsync(guildId, cancellationToken);
        return track is not null
            ? $"{track.Author} {track.Title}"
            : string.Empty;
    }
}