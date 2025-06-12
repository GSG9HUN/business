using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class ConnectionValidationResult(bool isValid, string errorKey, ILavalinkPlayer? connection)
{
    public bool isValid { get; } = isValid;
    public string errorKey { get; } = errorKey;
    public ILavalinkPlayer? connection { get; } = connection;
}