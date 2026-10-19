namespace DC_bot.BotControl.Payloads;

public sealed record QueueMoveToIndexCommandPayload(int TrackIndex, int TargetIndex);
