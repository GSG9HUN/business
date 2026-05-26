using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordMemberWrapper(DiscordMember discordMember, DiscordVoiceState? voiceState = null) : IDiscordMember
{
    public ulong Id => discordMember.Id;
    public bool IsBot => discordMember.IsBot;
    public string Username => discordMember.Username;
    public string Mention => discordMember.Mention;
    public IDiscordVoiceState VoiceState => new DiscordVoiceStateWrapper(voiceState ?? discordMember.VoiceState);

    public DiscordMember ToDiscordMember()
    {
        return discordMember;
    }
}
