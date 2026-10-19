namespace DC_bot.Interface.Discord;

public interface IDiscordGuild
{
    ulong Id { get; }
    string Name { get; }
    Task<IDiscordMember> GetMemberAsync(ulong id);
    Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
}