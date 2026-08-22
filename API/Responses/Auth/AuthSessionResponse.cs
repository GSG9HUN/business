namespace API.Responses.Auth;

public sealed record AuthSessionResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    long ExpiresAtMillis);