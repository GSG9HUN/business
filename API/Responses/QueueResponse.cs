namespace API.Responses;

public sealed class QueueResponse(ulong guildId, int trackCount, IReadOnlyList<QueueTrackResponse> tracks)
{
    public ulong GuildId { get; init; } = guildId;
    public int TrackCount { get; init; } = trackCount;
    public IReadOnlyList<QueueTrackResponse> Tracks { get; init; } = tracks;

    public void Deconstruct(out ulong guildId, out int trackCount, out IReadOnlyList<QueueTrackResponse> tracks)
    {
        guildId = GuildId;
        trackCount = TrackCount;
        tracks = Tracks;
    }
}
