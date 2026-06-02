using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;

namespace DC_bot.Wrapper;

public class SlashInteractionContextFactory : ISlashInteractionContextFactory
{
    public ISlashInteractionContext Create(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new SlashInteractionContextWrapper(context);
    }
}
