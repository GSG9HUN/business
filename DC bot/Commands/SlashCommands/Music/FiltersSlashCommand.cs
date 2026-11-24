using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Groups filter inspection, supported preset selection, and volume-preserving reset.</summary>
/// <param name="slashCommandExecutor">Executes the existing shared command pipeline.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
[Command(CommandNames.Filters)]
[Description("Inspect and configure playback filters")]
public sealed class FiltersSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{

    /// <summary>Shows confirmed filter state from the active Lavalink player.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <returns>The shared command execution.</returns>
    [Command("show")]
    [Description("Show confirmed playback filters")]
    public Task Show(SlashCommandContext context) => ExecuteAsync(contextFactory.Create(context), "show");

    /// <summary>Lists presets supported by the connected node.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <returns>The shared command execution.</returns>
    [Command("list")]
    [Description("List supported filter presets")]
    public Task List(SlashCommandContext context) => ExecuteAsync(contextFactory.Create(context), "list");

    /// <summary>Clears effects while retaining the player output volume.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <returns>The shared command execution.</returns>
    [Command("reset")]
    [Description("Reset all effects and retain output volume")]
    public Task Reset(SlashCommandContext context) => ExecuteAsync(contextFactory.Create(context), "reset");

    /// <summary>Replaces effects with the selected supported preset.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <param name="name">The advertised preset name.</param>
    /// <returns>The shared command execution.</returns>
    [Command("preset")]
    [Description("Apply a supported filter preset")]
    public Task Preset(
        SlashCommandContext context,
        [Parameter("name")][Description("Preset name from /filters list")] string name)
    {
        return ExecuteAsync(contextFactory.Create(context), "preset " + name.Trim());
    }

    /// <summary>Sets one bounded advanced effect, or shows its configuration when values are omitted.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <param name="effect">eq, rate, karaoke, rotation, channelmix, lowpass, tremolo, vibrato or distortion.</param>
    /// <param name="values">Space-separated numbers in effect order, off, or omitted to inspect.</param>
    /// <returns>The shared command execution.</returns>
    [Command("advanced")]
    [Description("Configure one advanced effect; use effect help for ranges")]
    public Task Advanced(
        SlashCommandContext context,
        [Parameter("effect")][Description("Effect name, or help")] string effect,
        [Parameter("values")][Description("Space-separated numbers, off, or omitted to inspect")] string? values = null)
    {
        return ExecuteAsync(contextFactory.Create(context), "advanced " + effect + " " + values);
    }

    /// <summary>Manages shared guild custom presets; saving/deleting requires Manage Guild.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="action">save, list, apply or delete.</param>
    /// <param name="name">The guild-local name; for list, an optional page number.</param>
    /// <param name="overwrite">Whether saving may overwrite an existing name.</param>
    /// <returns>The shared command execution.</returns>
    [Command("custom")]
    [Description("Manage shared guild presets: save/list/apply/delete")]
    public Task Custom(
        SlashCommandContext context,
        [Parameter("action")][Description("save, list, apply, delete")] string action,
        [Parameter("name")][Description("Preset name, or list page")] string? name = null,
        [Parameter("overwrite")][Description("Explicitly overwrite an existing saved preset")] bool overwrite = false)
    {
        return ExecuteAsync(contextFactory.Create(context), "custom " + action + " " + name
            + (overwrite ? " overwrite" : string.Empty));
    }

    /// <summary>Routes all filter subcommands through the existing text handler and localization.</summary>
    /// <param name="context">The adapted slash interaction.</param>
    /// <param name="argument">The filter subcommand and optional preset name.</param>
    /// <returns>The shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string argument)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            "filters", context, argument, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
