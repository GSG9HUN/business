using DC_bot.Interface;
using DC_bot.Logging;
using DC_bot.Service;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands;

public abstract class HelpSlashCommand : ApplicationCommandModule
{
    private const string CommandNameHelp = "help";
    private readonly ILogger<PlaySlashCommand> _logger = ServiceLocator.GetService<ILogger<PlaySlashCommand>>();

    [SlashCommand("help", "List the available commands")]
    public async Task Help(InteractionContext ctx)
    {
        _logger.CommandInvoked(CommandNameHelp);
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var commands = ServiceLocator.GetServices<ICommand>();
        var response = commands.Aggregate(String.Empty,
            (current, command) => current + $"{command.Name} : {command.Description}\n");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Available commands:\n{response}"));

        _logger.CommandExecuted(CommandNameHelp);
    }
}