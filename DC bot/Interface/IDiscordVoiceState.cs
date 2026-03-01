using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordVoiceState
{
    IDiscordChannel? Channel { get; }
    DiscordVoiceState ToDiscordVoiceState();
}