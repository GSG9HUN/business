using DC_bot.Interface.Service.Music.Tracks;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Queue.Models;

/// <summary>
/// Combines track, source, provider, and requester metadata for durable enqueueing.
/// </summary>
/// <param name="Track">The track whose identity and metadata are used by the operation.</param>
/// <param name="SourceQuery">The source query supplied for this operation.</param>
/// <param name="SourceSearchMode">The source search mode supplied for this operation.</param>
/// <param name="RequestedBy">The requested by supplied for this operation.</param>
public sealed record QueueTrackToEnqueue(
    ILavaLinkTrack Track,
    string? SourceQuery,
    TrackSearchMode? SourceSearchMode,
    string? RequestedBy);
