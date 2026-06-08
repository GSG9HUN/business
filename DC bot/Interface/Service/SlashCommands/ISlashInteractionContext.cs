using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Interface.Service.SlashCommands;

public interface ISlashInteractionContext
{
    ulong? GuildId { get; }
    IDiscordGuild? Guild { get; }
    IDiscordChannel Channel { get; }
    IDiscordUser User { get; }
    IDiscordMember? Member { get; }
    bool IsDeferred { get; }
    bool HasResponded { get; }

    Task DeferAsync();
    Task RespondAsync(string message);
    Task RespondAsync(DiscordEmbed embed);
    IDiscordMessage CreateMessage(string commandName, string? argument = null);
}
