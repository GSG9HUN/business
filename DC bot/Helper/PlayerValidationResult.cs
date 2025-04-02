using Lavalink4NET.Players;

namespace DC_bot.Helper;

public class PlayerValidationResult(bool isValid, ILavalinkPlayer? player)
{
    public bool isValid { get; } = isValid;
    public ILavalinkPlayer? player { get; } = player;
}