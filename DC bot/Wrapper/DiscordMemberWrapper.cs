using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordMemberWrapper(DiscordMember discordMember) : IDiscordMember
{
    public ulong Id => discordMember.Id;
    public bool IsBot => discordMember.IsBot;
    public string Username => discordMember.Username;
    public string Mention => discordMember.Mention;
    public IDiscordVoiceState VoiceState => new DiscordVoiceStateWrapper(discordMember.VoiceState);
    
    public DiscordMember ToDiscordMember() => discordMember;
}