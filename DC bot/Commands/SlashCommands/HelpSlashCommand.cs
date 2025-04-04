using DC_bot.Interface;
using DC_bot.Service;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands
{
    public abstract class HelpSlashCommand : ApplicationCommandModule
    {
        private readonly ILogger<PlaySlashCommand> _logger = ServiceLocator.GetService<ILogger<PlaySlashCommand>>();

        [SlashCommand("help", "List the available commands")]
        public async Task Help(InteractionContext ctx)
        {
            _logger.LogInformation("Help slash command invoked!");
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var commands = ServiceLocator.GetServices<ICommand>();
            var response = commands.Aggregate(String.Empty, (current, command) => current + $"{command.Name} : {command.Description}\n");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Available commands:\n{response}"));
            
            _logger.LogInformation("Help Command executed!");
        }
    }
}