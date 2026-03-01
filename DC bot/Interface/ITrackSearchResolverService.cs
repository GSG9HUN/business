using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface;

public interface ITrackSearchResolverService
{
    TrackSearchMode ResolveSearchMode(string input);
}