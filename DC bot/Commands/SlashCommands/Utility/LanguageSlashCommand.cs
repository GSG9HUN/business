using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Utility;

/// <summary>
/// Identifies a language offered by the language slash command.
/// </summary>
public enum SlashLanguage
{

    /// <summary>
    /// English is selected.
    /// </summary>
    [ChoiceDisplayName("en")]
    Eng,

    /// <summary>
    /// Hungarian is selected.
    /// </summary>
    [ChoiceDisplayName("hu")]
    Hu
}

/// <summary>
/// Adapts the language slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class LanguageSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "language";

    /// <summary>
    /// Adapts the language slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="language">The language supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("language")]
    [Description("Change the bot language")]
    public Task Language(SlashCommandContext context, [Parameter("language")][Description("Language to use")] SlashLanguage language)
    {
        return ExecuteAsync(contextFactory.Create(context), language);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="language">The language supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, SlashLanguage language)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, ToLanguageCode(language), RequireGuild: true, Defer: true), default);
    }

    /// <summary>
    /// Maps a supported language argument to its persisted code.
    /// </summary>
    /// <param name="language">The language supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string ToLanguageCode(SlashLanguage language)
    {
        return language switch
        {
            SlashLanguage.Eng => "en",
            SlashLanguage.Hu => "hu",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)};
    }
}
