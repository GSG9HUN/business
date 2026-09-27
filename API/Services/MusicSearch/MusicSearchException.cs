namespace API.Services.MusicSearch;

public sealed class MusicSearchException(
    string code, string message, int statusCode = 502, int? retryAfterSeconds = null) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
    public int? RetryAfterSeconds { get; } = retryAfterSeconds;
}
