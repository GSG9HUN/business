using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Exposes guild voting settings and the native Discord DJ-role picker.</summary>
/// <param name="executor">Executes the existing shared command pipeline.</param>
/// <param name="contexts">Adapts interaction contexts.</param>
[Command("music-config")]
[Description("Configure guild voting and DJ role")]
public sealed class MusicConfigSlashCommand(ISlashCommandExecutor executor, ISlashInteractionContextFactory contexts)
{

    /// <summary>Shows saved settings without requiring a player.</summary>
    [Command("show")]
    [Description("Show guild voting and DJ settings")]
    public Task Show(SlashCommandContext context) => ExecuteAsync(context, "show");

    /// <summary>Persists the guild vote-skip switch.</summary>
    [Command("vote-skip")]
    [Description("Enable or disable vote skip for this server")]
    public Task VoteSkip(SlashCommandContext context, [Description("Enable voting")] bool enabled)
        => ExecuteAsync(context, "vote-skip " + (enabled ? "on" : "off"));

    /// <summary>Sets a role selected by Discord, or clears it when omitted.</summary>
    [Command("dj-role")]
    [Description("Select the DJ role, or omit it to clear")]
    public Task DjRole(SlashCommandContext context, [Description("DJ role, omit to clear")] DiscordRole? role = null)
        => ExecuteAsync(context, "dj-role " + (role is null ? "clear" : role.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    /// <summary>Routes through the established text-command and deferred-response pipeline.</summary>
    private Task ExecuteAsync(SlashCommandContext context, string arguments)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest("music-config", contexts.Create(context), arguments,
            RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
