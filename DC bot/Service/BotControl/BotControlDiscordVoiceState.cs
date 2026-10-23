using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Adapts discord voice state data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="channel">The Discord channel used by the operation.</param>
internal sealed class BotControlDiscordVoiceState(IDiscordChannel channel) : IDiscordVoiceState
{
    public IDiscordChannel Channel => channel;

    /// <summary>
    /// Returns the underlying DSharpPlus voice state represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordVoiceState ToDiscordVoiceState()
    {
        throw new NotSupportedException("BotControl voice state is only used as an IDiscordVoiceState adapter.");
    }
}
