using DC_bot.Interface.Service.Music.Tracks;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Queue.Models;

public sealed record QueueTrackToEnqueue(
    ILavaLinkTrack Track,
    string? SourceQuery,
    TrackSearchMode? SourceSearchMode,
    string? RequestedBy);
