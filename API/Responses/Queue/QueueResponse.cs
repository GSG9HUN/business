namespace API.Responses.Queue;

public sealed record QueueResponse(string GuildId, int TrackCount, IReadOnlyList<QueueTrackResponse> Tracks);
