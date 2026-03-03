using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DC_bot.Helper;

public static class SlashCommandResponseHelper
{
    public static Task DeferAsync(InteractionContext ctx)
    {
        return ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
    }

    public static Task RespondAsync(InteractionContext ctx, string message)
    {
        return ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(message));
    }

    public static Task EditResponseAsync(InteractionContext ctx, string message)
    {
        return ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
    }

    public static async Task<bool> RequireGuildAsync(InteractionContext ctx, string message)
    {
        if (ctx.Guild != null)
        {
            return true;
        }

        await RespondAsync(ctx, message);
        return false;
    }

    public static async Task<bool> DeferAndRequireGuildAsync(InteractionContext ctx, string message)
    {
        await DeferAsync(ctx);
        if (ctx.Guild != null)
        {
            return true;
        }

        await EditResponseAsync(ctx, message);
        return false;
    }

    public static Task RespondAfterDeferAsync(InteractionContext ctx, string message)
    {
        return EditResponseAsync(ctx, message);
    }

    public static async Task<DiscordChannel?> TryGetVoiceChannelAfterDeferAsync(InteractionContext ctx, string message)
    {
        var channel = ctx.Member?.VoiceState?.Channel;
        if (channel != null)
        {
            return channel;
        }

        await RespondAfterDeferAsync(ctx, message);
        return null;
    }
}
