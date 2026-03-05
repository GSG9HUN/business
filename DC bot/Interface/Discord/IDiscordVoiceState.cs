using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

public interface IDiscordVoiceState
{
    IDiscordChannel? Channel { get; }
    DiscordVoiceState ToDiscordVoiceState();
}