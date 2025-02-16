using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordGuild
{
    ulong Id { get; }
    string Name { get; }
    Task<IDiscordMember> GetMemberAsync(ulong id);
    public DiscordGuild ToDiscordGuild();
    Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
}