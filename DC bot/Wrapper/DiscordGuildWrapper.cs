using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordGuildWrapper(DiscordGuild discordGuild) : IDiscordGuild
{
    public ulong Id => discordGuild.Id;
    public string Name => discordGuild.Name;

    public async Task<IDiscordMember> GetMemberAsync(ulong id)
    {
        var discordMember = discordGuild.Members.TryGetValue(id, out var cachedMember)
            ? cachedMember
            : await discordGuild.GetMemberAsync(id);

        discordGuild.VoiceStates.TryGetValue(id, out var voiceState);
        return new DiscordMemberWrapper(discordMember, voiceState);
    }

    public DiscordGuild ToDiscordGuild()
    {
        return discordGuild;
    }

    public async Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync()
    {
        var discordMembersWrapper = new List<IDiscordMember>();

        await foreach (var discordMember in discordGuild.GetAllMembersAsync())
        {
            discordMembersWrapper.Add(new DiscordMemberWrapper(discordMember));
        }

        return discordMembersWrapper;
    }
}
