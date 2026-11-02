namespace API.Services.MusicSearch;

/// <summary>
/// Describes a safe search error code, HTTP status, and optional retry delay.
/// </summary>
/// <param name="code">The code value for this operation.</param>
/// <param name="message">The message value for this operation.</param>
/// <param name="statusCode">The status code value for this operation.</param>
/// <param name="retryAfterSeconds">The retry after seconds value for this operation.</param>
public sealed class MusicSearchException(
    string code,
    string message,
    int statusCode = 502,
    int? retryAfterSeconds = null) : Exception(message)
{

    /// <summary>
    /// The machine-readable error code returned to the API client.
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// The HTTP status code associated with the search failure.
    /// </summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// The suggested retry delay in seconds, or null when unspecified.
    /// </summary>
    public int? RetryAfterSeconds { get; } = retryAfterSeconds;
}
