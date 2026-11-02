namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable queue remove command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "TrackNumber">The one-based track number used by the public queue or playlist operation.</param>
public sealed record QueueRemoveCommandPayload(int TrackNumber);
