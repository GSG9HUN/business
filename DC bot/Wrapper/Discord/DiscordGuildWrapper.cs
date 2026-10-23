using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord guild data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordGuild">The discord guild supplied for this operation.</param>
public class DiscordGuildWrapper(DiscordGuild discordGuild) : IDiscordGuild
{

    /// <inheritdoc />
    public ulong Id => discordGuild.Id;

    /// <inheritdoc />
    public string Name => discordGuild.Name;

    /// <summary>
    /// Resolves a guild member through the wrapped Discord API.
    /// </summary>
    /// <param name="id">The id supplied for this operation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<IDiscordMember> GetMemberAsync(ulong id)
    {
        var discordMember = discordGuild.Members.TryGetValue(id, out var cachedMember) ? cachedMember : await discordGuild.GetMemberAsync(id);
        discordGuild.VoiceStates.TryGetValue(id, out var voiceState);
        return new DiscordMemberWrapper(discordMember, voiceState);
    }

    /// <summary>
    /// Retrieves guild members through the wrapped Discord API.
    /// </summary>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
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
