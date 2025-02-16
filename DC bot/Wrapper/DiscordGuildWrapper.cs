using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordGuildWrapper(DiscordGuild discordGuild) : IDiscordGuild
{
    public ulong Id => discordGuild.Id;
    public string Name => discordGuild.Name;

    public async Task<IDiscordMember> GetMemberAsync(ulong id)
    {
        var discordMember = await discordGuild.GetMemberAsync(id);
        return new DiscordMemberWrapper(discordMember);
    }
    public DiscordGuild ToDiscordGuild()
    {
        return discordGuild;
    }

    public async Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync()
    {
        var discordMembers = await discordGuild.GetAllMembersAsync();
        var discordMembersWrapper = discordMembers.Select(x => new DiscordMemberWrapper(x)).ToList();
        return discordMembersWrapper;
    }
}