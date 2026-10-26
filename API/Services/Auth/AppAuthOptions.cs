namespace API.Services.Auth;

/// <summary>
/// Configures the signing key, issuer, and audience of mobile application access tokens.
/// </summary>
public sealed class AppAuthOptions
{

    /// <summary>
    /// The key used to sign application access tokens.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// The issuer value used for application access tokens.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The intended audience of application access tokens.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
}
