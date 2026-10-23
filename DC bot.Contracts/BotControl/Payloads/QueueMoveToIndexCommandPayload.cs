namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable queue move to index command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "TrackIndex">The track index value used by this storage contract.</param>
/// <param name = "TargetIndex">The target index value used by this storage contract.</param>
public sealed record QueueMoveToIndexCommandPayload(int TrackIndex, int TargetIndex);
