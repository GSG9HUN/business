using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class ConnectionValidationResult(bool isValid, string errorKey, ILavalinkPlayer? connection)
{
    public bool IsValid { get; } = isValid;
    public string ErrorKey { get; } = errorKey;
    public ILavalinkPlayer? Connection { get; } = connection;
}