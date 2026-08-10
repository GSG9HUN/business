namespace API.Responses.Queue;

public sealed class QueueTrackResponse(int position, string title, string author, TimeSpan duration, string trackUri, string? artworkUri)
{
    public int Position { get; init; } = position;
    public string Title { get; init; } = title;
    public string Author { get; init; } = author;
    public TimeSpan Duration { get; init; } = duration;
    public string TrackUri { get; init; } = trackUri;
    public string? ArtworkUri { get; init; } = artworkUri;

    public void Deconstruct(out int position, out string title, out string author, out TimeSpan duration, out string trackUri, out string? artworkUri)
    {
        position = Position;
        title = Title;
        author = Author;
        duration = Duration;
        trackUri = TrackUri;
        artworkUri = ArtworkUri;
    }
}
