namespace API.Responses.MusicSearch;

/// <summary>
/// Carries the music search error payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="ErrorCode">The error code value for this operation.</param>
/// <param name="ErrorMessage">The error message value for this operation.</param>
public sealed record MusicSearchErrorResponse(string ErrorCode, string ErrorMessage);
