namespace API.Requests.Playback;

/// <summary>Captures a mobile seek gesture without allowing it to target a replacement track.</summary>
/// <param name="PositionSeconds">Nonnegative absolute playback position in seconds.</param>
/// <param name="ExpectedPlaybackOperationId">Nonempty attempt identity from the playback snapshot.</param>
public sealed record SeekPlaybackRequest(int PositionSeconds, Guid ExpectedPlaybackOperationId);
