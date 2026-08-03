namespace API.Responses;

public sealed class PlaylistTrackResponse(int orderNumber, string title,
    string author, TimeSpan duration, string trackUri)
{
    public int OrderNumber { get; init; } = orderNumber;
    public string Title { get; init; } = title;
    public string Author { get; init; } = author;
    public TimeSpan Duration { get; init; } = duration;
    public string TrackUri { get; init; } = trackUri;

    public void Deconstruct(out int orderNumber, out string title, out string author, out TimeSpan duration, out string trackUri)
    {
        orderNumber = OrderNumber;
        title = Title;
        author = Author;
        duration = Duration;
        trackUri = TrackUri;
    }
}
