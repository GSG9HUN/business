using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Utility;

public enum SlashLanguage
{
    [ChoiceDisplayName("eng")]
    Eng,

    [ChoiceDisplayName("hu")]
    Hu
}

public class LanguageSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "language";

    [Command("language")]
    [Description("Change the bot language")]
    public Task Language(
        SlashCommandContext context,
        [Parameter("language")]
        [Description("Language to use")]
        SlashLanguage language)
    {
        return ExecuteAsync(contextFactory.Create(context), language);
    }

    public Task ExecuteAsync(ISlashInteractionContext context, SlashLanguage language)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            ToLanguageCode(language),
            RequireGuild: true,
            Defer: true));
    }

    private static string ToLanguageCode(SlashLanguage language)
    {
        return language switch
        {
            SlashLanguage.Eng => "eng",
            SlashLanguage.Hu => "hu",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
}
