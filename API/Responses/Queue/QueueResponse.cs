namespace API.Responses.Queue;

/// <summary>
/// Carries the queue payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="TrackCount">The track count value for this operation.</param>
/// <param name="Tracks">The tracks value for this operation.</param>
public sealed record QueueResponse(string GuildId, int TrackCount, IReadOnlyList<QueueTrackResponse> Tracks);
