namespace API.Responses.Auth;

/// <summary>
/// Carries the auth session payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="AccessToken">The access token value for this operation.</param>
/// <param name="RefreshToken">The refresh token value for this operation.</param>
/// <param name="ExpiresInSeconds">The expires in seconds value for this operation.</param>
/// <param name="ExpiresAtMillis">The expires at millis value for this operation.</param>
public sealed record AuthSessionResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds, long ExpiresAtMillis);
