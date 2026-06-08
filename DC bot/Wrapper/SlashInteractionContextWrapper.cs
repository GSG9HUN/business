using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class SlashInteractionContextWrapper(CommandContext context) : ISlashInteractionContext
{
    public ulong? GuildId => context.Guild?.Id;
    public IDiscordGuild? Guild => context.Guild is null ? null : new DiscordGuildWrapper(context.Guild);
    public IDiscordChannel Channel => new DiscordChannelWrapper(context.Channel, guild: context.Guild);
    public IDiscordUser User => new DiscordUserWrapper(context.User);
    public IDiscordMember? Member => context.Member is null ? null : new DiscordMemberWrapper(context.Member);
    public bool IsDeferred { get; private set; }
    public bool HasResponded { get; private set; }

    public async Task DeferAsync()
    {
        if (IsDeferred || HasResponded) return;

        await context.DeferResponseAsync();
        IsDeferred = true;
    }

    public async Task RespondAsync(string message)
    {
        if (IsDeferred)
        {
            await context.EditResponseAsync(message);
        }
        else if (!HasResponded)
        {
            await context.RespondAsync(message);
        }
        else
        {
            await context.FollowupAsync(message);
        }

        HasResponded = true;
    }

    public async Task RespondAsync(DiscordEmbed embed)
    {
        if (IsDeferred)
        {
            await context.EditResponseAsync(embed);
        }
        else if (!HasResponded)
        {
            await context.RespondAsync(embed);
        }
        else
        {
            await context.FollowupAsync(embed);
        }

        HasResponded = true;
    }

    public IDiscordMessage CreateMessage(string commandName, string? argument = null)
    {
        return new SlashInteractionMessageWrapper(
            string.IsNullOrWhiteSpace(argument) ? commandName : $"{commandName} {argument}",
            this);
    }
}
