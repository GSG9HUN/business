using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Search;

public interface ITrackSearchResolverService
{
    TrackSearchMode ResolveSearchMode(string input);
}