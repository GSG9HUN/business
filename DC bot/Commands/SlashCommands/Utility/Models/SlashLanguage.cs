using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

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
