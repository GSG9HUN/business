using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;

namespace DC_bot.Wrapper.SlashCommands;

/// <summary>
/// Represents the slash interaction context factory boundary and its associated operation data.
/// </summary>
public class SlashInteractionContextFactory : ISlashInteractionContextFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public ISlashInteractionContext Create(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new SlashInteractionContextWrapper(context);
    }
}
