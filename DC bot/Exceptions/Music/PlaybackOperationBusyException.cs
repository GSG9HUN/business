namespace DC_bot.Exceptions.Music;

public sealed class PlaybackOperationBusyException(string message) : InvalidOperationException(message);