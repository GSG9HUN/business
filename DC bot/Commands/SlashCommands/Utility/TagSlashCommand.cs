using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Utility;

public class TagSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "tag";

    [Command("tag")]
    [Description("You can tag someone")]
    public Task Tag(
        SlashCommandContext context,
        [Parameter("user")]
        [Description("Member you want to tag.")]
        DiscordMember member)
    {
        return ExecuteAsync(contextFactory.Create(context), member.Mention);
    }

    public Task ExecuteAsync(ISlashInteractionContext context, IDiscordMember member)
    {
        return ExecuteAsync(context, member.Mention);
    }

    private Task ExecuteAsync(ISlashInteractionContext context, string memberMention)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            memberMention,
            RequireGuild: true,
            Defer: true));
    }
}
