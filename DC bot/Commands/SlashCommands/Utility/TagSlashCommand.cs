using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Utility;

/// <summary>
/// Adapts the tag slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class TagSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "tag";

    /// <summary>
    /// Adapts the tag slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="member">The member supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("tag")]
    [Description("You can tag someone")]
    public Task Tag(SlashCommandContext context, [Parameter("user")][Description("Member you want to tag.")] DiscordMember member)
    {
        return ExecuteAsync(contextFactory.Create(context), member.Mention);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="memberMention">The member mention supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private Task ExecuteAsync(ISlashInteractionContext context, string memberMention)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, memberMention, RequireGuild: true, Defer: true), default);
    }
}
