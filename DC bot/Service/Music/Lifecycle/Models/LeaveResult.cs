namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Reports cleanup phases separately so partial cleanup is not treated as a confirmed disconnect.
/// </summary>
/// <param name="LocalInvalidation">The local invalidation supplied for this operation.</param>
/// <param name="Timer">The timer supplied for this operation.</param>
/// <param name="Subscription">The subscription supplied for this operation.</param>
/// <param name="Stop">The stop supplied for this operation.</param>
/// <param name="RemoteDisconnected">The remote disconnected supplied for this operation.</param>
/// <param name="VoicePersistence">The voice persistence supplied for this operation.</param>
/// <param name="CurrentTrackPersistence">The current track persistence supplied for this operation.</param>
public sealed record LeaveResult(
    LeavePhaseState LocalInvalidation,
    LeavePhaseState Timer,
    LeavePhaseState Subscription,
    LeavePhaseState Stop,
    LeavePhaseState RemoteDisconnected,
    LeavePhaseState VoicePersistence,
    LeavePhaseState CurrentTrackPersistence)
{

    /// <summary>
    /// Checks whether reconciliation is resolved sufficiently to continue playback.
    /// </summary>
    /// <param name="state">The state supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool Settled(LeavePhaseState state) => state is LeavePhaseState.Succeeded or LeavePhaseState.NotApplicable;

    /// <summary>
    /// Whether local invalidation succeeded and timer, subscription, disconnect, and persistence phases are settled; the stop phase is reported separately.
    /// </summary>
    public bool FullySucceeded => LocalInvalidation == LeavePhaseState.Succeeded
        && Settled(Timer)
        && Settled(Subscription)
        && Settled(RemoteDisconnected)
        && Settled(VoicePersistence)
        && Settled(CurrentTrackPersistence);
}
