namespace DC_bot.Exceptions.Music;

/// <summary>
/// Signals a competing or unresolved operation that prevents playback control.
/// </summary>
/// <param name="message">The message supplied for this operation.</param>
public sealed class PlaybackOperationBusyException(string message) : InvalidOperationException(message);
