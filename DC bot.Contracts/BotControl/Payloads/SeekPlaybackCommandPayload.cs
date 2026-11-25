namespace DC_bot.BotControl.Payloads;

/// <summary>Requests an absolute seek fenced to one playback attempt.</summary>
/// <param name="PositionSeconds">Nonnegative absolute position, strictly before the track end.</param>
/// <param name="ExpectedPlaybackOperationId">The playback attempt captured when the gesture started.</param>
public sealed record SeekPlaybackCommandPayload(int PositionSeconds, Guid ExpectedPlaybackOperationId);
