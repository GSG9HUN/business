namespace DC_bot.Interface.Service.SlashCommands;

public sealed record SlashCommandExecutionRequest(
    string CommandName,
    ISlashInteractionContext Context,
    string? Argument = null,
    bool RequireGuild = false,
    bool Defer = false,
    bool EnsureDeferredResponse = false);
