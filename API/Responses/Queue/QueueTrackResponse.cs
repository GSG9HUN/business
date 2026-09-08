namespace API.Responses.Queue;

public sealed record QueueTrackResponse(
    int Position,
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri);
