namespace DC_bot.Interface.Service.SlashCommands;

/// <summary>
/// Specifies command arguments and the acknowledgement policy for slash execution.
/// </summary>
/// <param name="CommandName">The command name supplied for this operation.</param>
/// <param name="Context">The slash interaction context dependency used by this boundary.</param>
/// <param name="Argument">The argument supplied for this operation.</param>
/// <param name="RequireGuild">The require guild supplied for this operation.</param>
/// <param name="Defer">The defer supplied for this operation.</param>
/// <param name="EnsureDeferredResponse">The ensure deferred response supplied for this operation.</param>
public sealed record SlashCommandExecutionRequest(
    string CommandName,
    ISlashInteractionContext Context,
    string? Argument = null,
    bool RequireGuild = false,
    bool Defer = false,
    bool EnsureDeferredResponse = false);
