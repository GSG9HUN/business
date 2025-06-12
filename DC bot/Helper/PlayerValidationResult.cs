using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class PlayerValidationResult(bool isValid, string errorKey, ILavalinkPlayer? player)
{
    public bool isValid { get; } = isValid;
    public string errorKey { get; } = errorKey;
    public ILavalinkPlayer? player { get; } = player;
}