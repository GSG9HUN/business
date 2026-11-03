using DSharpPlus.Commands;

namespace DC_bot.Interface.Service.SlashCommands;

/// <summary>
/// Defines the slash interaction context factory boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ISlashInteractionContextFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    ISlashInteractionContext Create(CommandContext context);
}
