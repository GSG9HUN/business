using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class PlayerValidationResult(bool isValid, string errorKey, ILavalinkPlayer? player)
{
    public bool IsValid { get; } = isValid;
    public string ErrorKey { get; } = errorKey;
    public ILavalinkPlayer? Player { get; } = player;
}