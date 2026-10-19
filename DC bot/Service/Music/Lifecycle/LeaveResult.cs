namespace DC_bot.Service.Music.Lifecycle;

public enum LeavePhaseState
{
    NotAttempted,
    Succeeded,
    Failed,
    Unknown,
    NotApplicable
}

public sealed record LeaveResult(
    LeavePhaseState LocalInvalidation,
    LeavePhaseState Timer,
    LeavePhaseState Subscription,
    LeavePhaseState Stop,
    LeavePhaseState RemoteDisconnected,
    LeavePhaseState VoicePersistence,
    LeavePhaseState CurrentTrackPersistence)
{
    private static bool Settled(LeavePhaseState state) =>
        state is LeavePhaseState.Succeeded or LeavePhaseState.NotApplicable;

    public bool FullySucceeded =>
        LocalInvalidation == LeavePhaseState.Succeeded &&
        Settled(Timer) &&
        Settled(Subscription) &&
        Settled(RemoteDisconnected) &&
        Settled(VoicePersistence) &&
        Settled(CurrentTrackPersistence);
}