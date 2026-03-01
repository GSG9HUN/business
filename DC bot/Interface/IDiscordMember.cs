using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordMember
{
    ulong Id { get; }
    bool IsBot { get; }
    string Username { get; }
    string Mention { get; }
    IDiscordVoiceState? VoiceState { get; }

    DiscordMember ToDiscordMember();
}