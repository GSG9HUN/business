using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordMember
{
    ulong Id { get; } // Felhasználó egyedi azonosítója
    bool IsBot { get; } // Bot-e a felhasználó
    string Username { get; } // Felhasználó neve
    string Mention { get; } // Megemlítés Discord üzenetben (pl. <@123456789>)
    IDiscordVoiceState? VoiceState { get; } // Voice állapot, lehet null

    DiscordMember ToDiscordMember();
}