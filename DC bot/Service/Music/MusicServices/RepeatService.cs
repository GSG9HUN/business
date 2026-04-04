using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class RepeatService(
    IPlaybackStateRepository playbackStateRepository,
    IRepeatListRepository repeatListRepository, 
    ILogger<RepeatService> logger) : IRepeatService
{
    public async Task InitAsync(ulong guildId)
    {
        await playbackStateRepository.GetOrCreateAsync(guildId);
    }

    public async Task<bool> IsRepeatingAsync(ulong guildId)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        return state.IsRepeating;
    }

    public async Task SetRepeatingAsync(ulong guildId, bool value)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        await playbackStateRepository.SetRepeatStateAsync(guildId, value, state.IsRepeatingList);
    }

    public async Task<bool> IsRepeatingListAsync(ulong guildId)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        return state.IsRepeatingList;
    }

    public async Task SetRepeatingListAsync(ulong guildId, bool value)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        await playbackStateRepository.SetRepeatStateAsync(guildId, state.IsRepeating, value);

        if (!value)
        {
            await repeatListRepository.ClearAsync(guildId);
        }
    }

    public async Task SaveRepeatListSnapshotAsync(
        ulong guildId,
        ILavaLinkTrack? currentTrack,
        IReadOnlyCollection<ILavaLinkTrack> queuedTracks)
    {
        ArgumentNullException.ThrowIfNull(queuedTracks);

        var trackIdentifiers = new List<string>(queuedTracks.Count + (currentTrack is null ? 0 : 1));
        if (currentTrack is not null)
        {
            trackIdentifiers.Add(currentTrack.ToString());
        }

        trackIdentifiers.AddRange(queuedTracks.Select(track => track.ToString()));

        await repeatListRepository.ReplaceAsync(guildId, trackIdentifiers);
    }

    public async Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueueAsync(ulong guildId)
    {
        var trackIdentifiers = await repeatListRepository.GetTrackIdentifiersAsync(guildId);
        var validTracks = new List<ILavaLinkTrack>();
       foreach (var identifier in trackIdentifiers)
        {
            try
            {
                var track = LavalinkTrack.Parse(identifier, null);
                validTracks.Add(new LavaLinkTrackWrapper(track));
            }
            catch (Exception ex)
            { 
                logger.LogError(ex, "Corrupted track identifier: {GuildId}. Identifier: {Identifier}", guildId, identifier);
            }
        }

        return validTracks;
    }
}