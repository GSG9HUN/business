namespace DC_bot.Configuration;

/// <summary>
/// Defines deadlines for join, leave, cleanup, and pending-start reconciliation.
/// </summary>
public sealed class PlaybackLifecycleOptions
{

    /// <summary>
    /// The maximum time allowed for the join lifecycle operation.
    /// </summary>
    public TimeSpan JoinDeadline { get; set; }

    /// <summary>
    /// The maximum time allowed for the leave lifecycle operation.
    /// </summary>
    public TimeSpan LeaveDeadline { get; set; }
}
