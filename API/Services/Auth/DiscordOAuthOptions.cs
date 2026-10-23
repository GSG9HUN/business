namespace API.Services.Auth;

/// <summary>
/// Configures Discord OAuth credentials and the browser and mobile callback addresses.
/// </summary>
public sealed class DiscordOAuthOptions
{

    /// <summary>
    /// The Discord OAuth application identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The Discord OAuth application secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The configured redirect URI for the standard OAuth callback.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// The configured redirect URI for the Android OAuth callback.
    /// </summary>
    public string AndroidRedirectUri { get; set; } = "myapp://auth";
}
