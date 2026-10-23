namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord guild boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordGuild
{

    /// <summary>
    /// The Discord identifier of this object.
    /// </summary>
    ulong Id { get; }

    /// <summary>
    /// The display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Resolves a guild member through the wrapped Discord API.
    /// </summary>
    /// <param name="id">The id supplied for this operation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<IDiscordMember> GetMemberAsync(ulong id);

    /// <summary>
    /// Retrieves guild members through the wrapped Discord API.
    /// </summary>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
}
