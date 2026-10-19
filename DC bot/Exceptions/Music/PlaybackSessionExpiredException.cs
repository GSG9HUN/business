namespace DC_bot.Exceptions.Music;

public sealed class PlaybackSessionExpiredException() : InvalidOperationException("The playback session has expired.");