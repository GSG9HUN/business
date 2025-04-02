using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class ConnectionValidationResult(bool isValid, ILavalinkPlayer? connection)
{
    public bool isValid { get; } = isValid;
    public ILavalinkPlayer? connection { get; } = connection;
}