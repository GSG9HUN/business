namespace API.Requests.Auth;

/// <summary>
/// Carries client input for refresh processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="RefreshToken">The refresh token value for this operation.</param>
public sealed record RefreshRequest(string RefreshToken);
