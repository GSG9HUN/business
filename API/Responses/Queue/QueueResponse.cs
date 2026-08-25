namespace API.Responses.Queue;

public sealed record QueueResponse(ulong GuildId, int TrackCount, IReadOnlyList<QueueTrackResponse> Tracks);