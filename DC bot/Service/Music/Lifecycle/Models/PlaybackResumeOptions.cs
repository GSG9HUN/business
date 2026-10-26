namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Describes the confirmed playback position and pause state to restore after reconnecting.
/// </summary>
/// <param name="Position">The source-track position from the latest checkpoint.</param>
/// <param name="IsPaused">Whether playback must start paused.</param>
/// <remarks>
/// Applies to the same durable queue occurrence. Position is source-track time; non-seekable sources use zero.
/// A paused recovery sends track, position, and pause in one request rather than playing before pausing.
/// </remarks>
public sealed record PlaybackResumeOptions(TimeSpan Position, bool IsPaused);
