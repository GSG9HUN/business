using DC_bot.Logging;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands;

public abstract class TagSlashCommand : ApplicationCommandModule
{
    private const string CommandNameTag = "tag";

    // Property injection supported by DSharpPlus SlashCommands
    public ILogger<TagSlashCommand> Logger { private get; set; } = null!;

    [SlashCommand("tag", "You can tag someone")]
    public Task Tag(InteractionContext ctx,
        [Option("name", "Name of the member you want to tag.")]
        string username)
    {
        Logger.CommandInvoked(CommandNameTag);
        /*if (!await SlashCommandResponseHelper.DeferAndRequireGuildAsync(ctx, "This command can only be used in a server."))
        {
            return;
        }

        var allMembers = await ctx.Guild.GetAllMembersAsync();
        var member = allMembers.FirstOrDefault(m =>
            m.Username.Contains(username, StringComparison.OrdinalIgnoreCase));
        if (member == null)
        {
            await SlashCommandResponseHelper.RespondAfterDeferAsync(ctx, $"User '{username}' not found.");
            return;
        }

        await SlashCommandResponseHelper.RespondAfterDeferAsync(ctx, $"You tagged: {member.Mention}");*/
        Logger.CommandExecuted(CommandNameTag);
        return Task.CompletedTask;
    }
}
