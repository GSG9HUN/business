namespace DC_bot.Interface.Service.Music;

public sealed record PlaybackControlResult(
    bool Success,
    string Message,
    string? ErrorCode = null)
{
    public static PlaybackControlResult Succeeded(string message)
    {
        return new PlaybackControlResult(true, message);
    }

    public static PlaybackControlResult Failed(string message, string errorCode)
    {
        return new PlaybackControlResult(false, message, errorCode);
    }
}
