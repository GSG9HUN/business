using DC_bot.Interface.Discord;

namespace DC_bot.Service.ReactionHandler;

public sealed record ReactionContext(IDiscordMember Member, IDiscordMessage Message, ulong GuildId);
