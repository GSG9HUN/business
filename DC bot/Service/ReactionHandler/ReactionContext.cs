using DC_bot.Interface.Discord;

namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Carries the message, guild, member, and channel used by a music-control reaction.
/// </summary>
/// <param name="Member">The member whose guild and voice context are used by the operation.</param>
/// <param name="Message">The message supplying command context and the response channel.</param>
/// <param name="GuildId">The Discord guild whose state or operation is addressed.</param>
public sealed record ReactionContext(IDiscordMember Member, IDiscordMessage Message, ulong GuildId);
