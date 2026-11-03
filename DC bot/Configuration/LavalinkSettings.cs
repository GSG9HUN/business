namespace DC_bot.Configuration;

/// <summary>
/// Defines the Lavalink endpoint and authentication used at startup.
/// </summary>
public sealed class LavalinkSettings
{

    /// <summary>
    /// The hostname of the Lavalink server.
    /// </summary>
    public string? Hostname { get; init; }

    /// <summary>
    /// The port on which the Lavalink server accepts connections.
    /// </summary>
    public int Port { get; init; } = 2333;

    /// <summary>
    /// Whether the Lavalink connection uses TLS.
    /// </summary>
    public bool Secured { get; init; }

    /// <summary>
    /// The password used to authenticate requests to Lavalink.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
