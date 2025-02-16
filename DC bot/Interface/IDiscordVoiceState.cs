using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordVoiceState
{
    IDiscordChannel?
        Channel { get; } // A voice csatorna, ahol a felhasználó tartózkodik (null, ha nincs voice channelben)

    DiscordVoiceState ToDiscordVoiceState();
}