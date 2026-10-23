using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Wrapper.Discord;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.SlashCommands;

/// <summary>
/// Adapts slash interaction context data and operations to the bot&apos;s Discord boundary.
/// </summary>
public class SlashInteractionContextWrapper : ISlashInteractionContext
{
    private readonly ISlashCommandContextAdapter _context;

    /// <summary>
    /// Initializes SlashInteractionContextWrapper with the supplied dependencies and settings.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    public SlashInteractionContextWrapper(CommandContext context) : this(new DSharpPlusSlashCommandContextAdapter(context))
    {
    }

    /// <summary>
    /// Initializes SlashInteractionContextWrapper with the supplied dependencies and settings.
    /// </summary>
    /// <param name="context">The slash command context adapter dependency used by this boundary.</param>
    internal SlashInteractionContextWrapper(ISlashCommandContextAdapter context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ulong? GuildId => _context.Guild?.Id;

    /// <inheritdoc />
    public IDiscordGuild? Guild => _context.Guild is null ? null : new DiscordGuildWrapper(_context.Guild);

    /// <inheritdoc />
    public IDiscordChannel Channel => new DiscordChannelWrapper(_context.Channel, guild: _context.Guild);

    /// <inheritdoc />
    public IDiscordUser User => new DiscordUserWrapper(_context.User);

    /// <inheritdoc />
    public IDiscordMember? Member => _context.Member is null ? null : new DiscordMemberWrapper(_context.Member);

    /// <inheritdoc />
    public bool IsDeferred { get; private set; }

    /// <inheritdoc />
    public bool HasResponded { get; private set; }

    /// <summary>
    /// Acknowledges an interaction so its response can be completed after command execution.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task DeferAsync()
    {
        if (IsDeferred || HasResponded)
        {
            return;
        }

        await _context.DeferResponseAsync();
        IsDeferred = true;
    }

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Creates a synthetic command message whose responses are delivered through the wrapped slash interaction.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public IDiscordMessage CreateMessage(string commandName, string? argument = null)
    {
        return new SlashInteractionMessageWrapper(string.IsNullOrWhiteSpace(argument) ? commandName : $"{commandName} {argument}", this);
    }

    /// <summary>
    /// Represents the d sharp plus slash command context adapter boundary and its associated operation data.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    private sealed class DSharpPlusSlashCommandContextAdapter(CommandContext context) : ISlashCommandContextAdapter
    {
        public DiscordGuild? Guild => context.Guild;
        public DiscordChannel Channel => context.Channel;
        public DiscordUser User => context.User;
        public DiscordMember? Member => context.Member;

        /// <summary>
        /// Defers a slash response using the requested visibility policy.
        /// </summary>
        /// <returns>A task representing completion of this operation.</returns>
        public Task DeferResponseAsync()
        {
            return context.DeferResponseAsync()
                .AsTask();
        }

        /// <summary>
        /// Sends a response through the wrapped message or interaction boundary.
        /// </summary>
        /// <param name="message">The message supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task RespondAsync(string message)
        {
            return context.RespondAsync(message)
                .AsTask();
        }

        /// <summary>
        /// Sends a response through the wrapped message or interaction boundary.
        /// </summary>
        /// <param name="embed">The embed supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task RespondAsync(DiscordEmbed embed)
        {
            return context.RespondAsync(embed)
                .AsTask();
        }

        /// <summary>
        /// Updates the previously acknowledged interaction response.
        /// </summary>
        /// <param name="message">The message supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task EditResponseAsync(string message)
        {
            return context.EditResponseAsync(message)
                .AsTask();
        }

        /// <summary>
        /// Updates the previously acknowledged interaction response.
        /// </summary>
        /// <param name="embed">The embed supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task EditResponseAsync(DiscordEmbed embed)
        {
            return context.EditResponseAsync(embed)
                .AsTask();
        }

        /// <summary>
        /// Sends a follow-up message after interaction acknowledgement.
        /// </summary>
        /// <param name="message">The message supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task FollowupAsync(string message)
        {
            return context.FollowupAsync(message)
                .AsTask();
        }

        /// <summary>
        /// Sends a follow-up message after interaction acknowledgement.
        /// </summary>
        /// <param name="embed">The embed supplied for this operation.</param>
        /// <returns>A task representing completion of this operation.</returns>
        public Task FollowupAsync(DiscordEmbed embed)
        {
            return context.FollowupAsync(embed)
                .AsTask();
        }
    }
}

/// <summary>
/// Defines the slash command context adapter boundary so callers are independent of its concrete implementation.
/// </summary>
internal interface ISlashCommandContextAdapter
{
    DiscordGuild? Guild { get; }

    DiscordChannel Channel { get; }

    DiscordUser User { get; }

    DiscordMember? Member { get; }

    /// <summary>
    /// Defers a slash response using the requested visibility policy.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    Task DeferResponseAsync();

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(string message);

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(DiscordEmbed embed);

    /// <summary>
    /// Updates the previously acknowledged interaction response.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task EditResponseAsync(string message);

    /// <summary>
    /// Updates the previously acknowledged interaction response.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task EditResponseAsync(DiscordEmbed embed);

    /// <summary>
    /// Sends a follow-up message after interaction acknowledgement.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task FollowupAsync(string message);

    /// <summary>
    /// Sends a follow-up message after interaction acknowledgement.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task FollowupAsync(DiscordEmbed embed);
}
