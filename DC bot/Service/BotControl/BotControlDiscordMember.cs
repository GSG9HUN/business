using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

internal sealed class BotControlDiscordMember(
    DiscordMember discordMember,
    IDiscordChannel responseChannel)
    : IDiscordMember
{
    public ulong Id => discordMember.Id;
    public bool IsBot => discordMember.IsBot;
    public string Username => discordMember.Username;
    public string Mention => discordMember.Mention;
    public IDiscordVoiceState? VoiceState { get; } = new BotControlDiscordVoiceState(responseChannel);

    public DiscordMember ToDiscordMember()
    {
        return discordMember;
    }
}
