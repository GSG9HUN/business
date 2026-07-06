using DC_bot.Wrapper;
using DSharpPlus.Entities;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class SlashInteractionContextWrapperTests
{
    [Fact]
    public async Task RespondAsync_WhenDeferred_EditsDeferredResponse()
    {
        var adapter = new RecordingSlashCommandContextAdapter();
        var context = new SlashInteractionContextWrapper(adapter);

        await context.DeferAsync();
        await context.RespondAsync("done");

        Assert.True(context.IsDeferred);
        Assert.True(context.HasResponded);
        Assert.Equal(["defer", "edit:done"], adapter.Calls);
    }

    [Fact]
    public async Task RespondAsync_WhenAlreadyResponded_SendsFollowup()
    {
        var adapter = new RecordingSlashCommandContextAdapter();
        var context = new SlashInteractionContextWrapper(adapter);

        await context.RespondAsync("first");
        await context.RespondAsync("second");

        Assert.False(context.IsDeferred);
        Assert.True(context.HasResponded);
        Assert.Equal(["respond:first", "followup:second"], adapter.Calls);
    }

    [Fact]
    public async Task DeferAsync_WhenAlreadyResponded_DoesNotDefer()
    {
        var adapter = new RecordingSlashCommandContextAdapter();
        var context = new SlashInteractionContextWrapper(adapter);

        await context.RespondAsync("first");
        await context.DeferAsync();

        Assert.False(context.IsDeferred);
        Assert.True(context.HasResponded);
        Assert.Equal(["respond:first"], adapter.Calls);
    }

    private sealed class RecordingSlashCommandContextAdapter : ISlashCommandContextAdapter
    {
        public List<string> Calls { get; } = [];
        public DiscordGuild? Guild => null;
        public DiscordChannel Channel => throw new NotSupportedException();
        public DiscordUser User => throw new NotSupportedException();
        public DiscordMember? Member => null;

        public Task DeferResponseAsync()
        {
            Calls.Add("defer");
            return Task.CompletedTask;
        }

        public Task RespondAsync(string message)
        {
            Calls.Add($"respond:{message}");
            return Task.CompletedTask;
        }

        public Task RespondAsync(DiscordEmbed embed)
        {
            Calls.Add("respond:embed");
            return Task.CompletedTask;
        }

        public Task EditResponseAsync(string message)
        {
            Calls.Add($"edit:{message}");
            return Task.CompletedTask;
        }

        public Task EditResponseAsync(DiscordEmbed embed)
        {
            Calls.Add("edit:embed");
            return Task.CompletedTask;
        }

        public Task FollowupAsync(string message)
        {
            Calls.Add($"followup:{message}");
            return Task.CompletedTask;
        }

        public Task FollowupAsync(DiscordEmbed embed)
        {
            Calls.Add("followup:embed");
            return Task.CompletedTask;
        }
    }
}