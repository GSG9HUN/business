using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class SlashInteractionContextWrapper : ISlashInteractionContext
{
    private readonly ISlashCommandContextAdapter _context;

    public SlashInteractionContextWrapper(CommandContext context)
        : this(new DSharpPlusSlashCommandContextAdapter(context))
    {
    }

    internal SlashInteractionContextWrapper(ISlashCommandContextAdapter context)
    {
        _context = context;
    }

    public ulong? GuildId => _context.Guild?.Id;
    public IDiscordGuild? Guild => _context.Guild is null ? null : new DiscordGuildWrapper(_context.Guild);
    public IDiscordChannel Channel => new DiscordChannelWrapper(_context.Channel, guild: _context.Guild);
    public IDiscordUser User => new DiscordUserWrapper(_context.User);
    public IDiscordMember? Member => _context.Member is null ? null : new DiscordMemberWrapper(_context.Member);
    public bool IsDeferred { get; private set; }
    public bool HasResponded { get; private set; }

    public async Task DeferAsync()
    {
        if (IsDeferred || HasResponded) return;

        await _context.DeferResponseAsync();
        IsDeferred = true;
    }

    public async Task RespondAsync(string message)
    {
        if (IsDeferred)
        {
            await _context.EditResponseAsync(message);
        }
        else if (!HasResponded)
        {
            await _context.RespondAsync(message);
        }
        else
        {
            await _context.FollowupAsync(message);
        }

        HasResponded = true;
    }

    public async Task RespondAsync(DiscordEmbed embed)
    {
        if (IsDeferred)
        {
            await _context.EditResponseAsync(embed);
        }
        else if (!HasResponded)
        {
            await _context.RespondAsync(embed);
        }
        else
        {
            await _context.FollowupAsync(embed);
        }

        HasResponded = true;
    }

    public IDiscordMessage CreateMessage(string commandName, string? argument = null)
    {
        return new SlashInteractionMessageWrapper(
            string.IsNullOrWhiteSpace(argument) ? commandName : $"{commandName} {argument}",
            this);
    }

    private sealed class DSharpPlusSlashCommandContextAdapter(CommandContext context) : ISlashCommandContextAdapter
    {
        public DiscordGuild? Guild => context.Guild;
        public DiscordChannel Channel => context.Channel;
        public DiscordUser User => context.User;
        public DiscordMember? Member => context.Member;

        public Task DeferResponseAsync()
        {
            return context.DeferResponseAsync().AsTask();
        }

        public Task RespondAsync(string message)
        {
            return context.RespondAsync(message).AsTask();
        }

        public Task RespondAsync(DiscordEmbed embed)
        {
            return context.RespondAsync(embed).AsTask();
        }

        public Task EditResponseAsync(string message)
        {
            return context.EditResponseAsync(message).AsTask();
        }

        public Task EditResponseAsync(DiscordEmbed embed)
        {
            return context.EditResponseAsync(embed).AsTask();
        }

        public Task FollowupAsync(string message)
        {
            return context.FollowupAsync(message).AsTask();
        }

        public Task FollowupAsync(DiscordEmbed embed)
        {
            return context.FollowupAsync(embed).AsTask();
        }
    }
}

internal interface ISlashCommandContextAdapter
{
    DiscordGuild? Guild { get; }
    DiscordChannel Channel { get; }
    DiscordUser User { get; }
    DiscordMember? Member { get; }

    Task DeferResponseAsync();
    Task RespondAsync(string message);
    Task RespondAsync(DiscordEmbed embed);
    Task EditResponseAsync(string message);
    Task EditResponseAsync(DiscordEmbed embed);
    Task FollowupAsync(string message);
    Task FollowupAsync(DiscordEmbed embed);
}
