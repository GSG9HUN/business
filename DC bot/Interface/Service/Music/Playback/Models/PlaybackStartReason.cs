namespace DC_bot.Interface.Service.Music.Playback.Models;

/// <summary>
/// Identifies the action that requested a playback start.
/// </summary>
public enum PlaybackStartReason
{

    /// <summary>
    /// Playback starts for the initial requested track.
    /// </summary>
    First,

    /// <summary>
    /// Playback advances to the next queued track.
    /// </summary>
    QueueNext,

    /// <summary>
    /// Playback starts from the queue after joining a voice channel.
    /// </summary>
    JoinQueue,

    /// <summary>
    /// The current track is started again in single-track repeat mode.
    /// </summary>
    RepeatCurrent,

    /// <summary>
    /// Playback starts a track from the repeated playback batch.
    /// </summary>
    RepeatList,

    /// <summary>
    /// Playback starts a replacement or reloaded track.
    /// </summary>
    Reload,

    /// <summary>
    /// Playback moves to the previous track in the active batch.
    /// </summary>
    Previous,
    
    /// <summary>
    /// Restores an existing queue item after a confirmed voice disconnect.
    /// </summary>
    Recovery
}
