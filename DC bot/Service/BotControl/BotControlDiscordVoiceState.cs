using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

internal sealed class BotControlDiscordVoiceState(IDiscordChannel channel) : IDiscordVoiceState
{
    public IDiscordChannel? Channel => channel;

    public DiscordVoiceState ToDiscordVoiceState()
    {
        throw new NotSupportedException("BotControl voice state is only used as an IDiscordVoiceState adapter.");
    }
}
