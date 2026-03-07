namespace DC_bot.Configuration;

public sealed class LavalinkSettings
{
    public string? Hostname { get; init; }
    public int Port { get; init; } = 2333;
    public bool Secured { get; init; }
    public string Password { get; init; } = string.Empty;
}

