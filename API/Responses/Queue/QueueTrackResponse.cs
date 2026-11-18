namespace API.Responses.Queue;

/// <summary>
/// Carries the queue track payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="Position">The position value for this operation.</param>
/// <param name="Title">The title value for this operation.</param>
/// <param name="Author">The author value for this operation.</param>
/// <param name="Duration">The duration value for this operation.</param>
/// <param name="TrackUri">The track uri value for this operation.</param>
/// <param name="ArtworkUri">The artwork uri value for this operation.</param>
/// <param name="RequestedBy">The requested by value for this operation.</param>
public sealed record QueueTrackResponse(
    int Position,
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri,
    string? RequestedBy)
{

    /// <summary>Identifies this queue occurrence independently of its source URL.</summary>
    public string? QueueItemId { get; init; }

    /// <summary>Identifies the originating enqueue operation.</summary>
    public string? BatchId { get; init; }

    /// <summary>Gets the original zero-based order within the batch.</summary>
    public int BatchPosition { get; init; }

    /// <summary>Gets Queued, Playing, Played, or Skipped for this occurrence.</summary>
    public string State { get; init; } = "Queued";

    /// <summary>Gets the timestamp used to order played and skipped occurrences in a saved snapshot.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
