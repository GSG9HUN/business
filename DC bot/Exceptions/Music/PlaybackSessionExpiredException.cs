namespace DC_bot.Exceptions.Music;

/// <summary>
/// Signals that work no longer owns the current guild player generation.
/// </summary>
public sealed class PlaybackSessionExpiredException() : InvalidOperationException("The playback session has expired.");
