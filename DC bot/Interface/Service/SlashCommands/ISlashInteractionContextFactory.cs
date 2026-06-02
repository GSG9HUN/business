using DSharpPlus.Commands;

namespace DC_bot.Interface.Service.SlashCommands;

public interface ISlashInteractionContextFactory
{
    ISlashInteractionContext Create(CommandContext context);
}
