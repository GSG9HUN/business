using DC_bot.Helper;
using DSharpPlus.SlashCommands;

namespace DC_bot.Commands.SlashCommands;

public abstract class TagSlashCommand : ApplicationCommandModule
{
    [SlashCommand("tag", "You can tag someone")]
    public async Task Tag(InteractionContext ctx,
        [Option("name", "Name of the member you want to tag.")]
        string username)
    {
        if (!await SlashCommandResponseHelper.DeferAndRequireGuildAsync(ctx, "This command can only be used in a server."))
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

        await SlashCommandResponseHelper.RespondAfterDeferAsync(ctx, $"You tagged: {member.Mention}");
    }
}