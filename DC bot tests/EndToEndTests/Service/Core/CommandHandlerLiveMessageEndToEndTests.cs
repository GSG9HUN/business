namespace DC_bot_tests.EndToEndTests.Service.Core;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class CommandHandlerLiveMessageEndToEndTests : CommandHandlerEndToEndTestBase
{
    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Test_Message()
    {
        if (!CanRun()) return;
        CommandHandlerService.RegisterHandler(DiscordClient);

        var channel = await GetTestChannelAsync(DiscordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await CommandHandlerService.HandleEventAsync(DiscordClient,
            DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
        var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            testMessage.Id,
            message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase),
            "Pong response",
            limit: 5);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);

        CommandHandlerService.UnregisterHandler(DiscordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        if (!CanRun()) return;
        CommandHandlerService.RegisterHandler(DiscordClient);

        var channel = await GetTestChannelAsync(DiscordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await CommandHandlerService.HandleEventAsync(DiscordClient,
            DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
        var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            testMessage.Id,
            message => message.Content.Contains("Unknown command.", StringComparison.OrdinalIgnoreCase),
            "unknown command response",
            limit: 5);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);

        CommandHandlerService.UnregisterHandler(DiscordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Help_Message()
    {
        if (!CanRun()) return;
        CommandHandlerService.RegisterHandler(DiscordClient);

        try
        {
            var channel = await GetTestChannelAsync(DiscordClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var testMessage = await channel.SendMessageAsync("!help");

            await CommandHandlerService.HandleEventAsync(DiscordClient,
                DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
            var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
                channel,
                testMessage.Id,
                message => message.Content.Contains("Available commands:", StringComparison.OrdinalIgnoreCase),
                "help command response",
                limit: 5);
            Assert.Contains("ping", response.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("help", response.Content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CommandHandlerService.UnregisterHandler(DiscordClient);
        }
    }

    [Fact]
    public async Task HandleCommandAsync_WhenMessageHasNoPrefix_DoesNotRespond()
    {
        if (!CanRun()) return;
        CommandHandlerService.RegisterHandler(DiscordClient);

        try
        {
            var channel = await GetTestChannelAsync(DiscordClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var marker = $"e2e-no-prefix-{Guid.NewGuid():N}";
            var markerMessage = await channel.SendMessageAsync(marker);

            await CommandHandlerService.HandleEventAsync(DiscordClient,
                DiscordEventArgsFactory.CreateMessageCreated(markerMessage, guild));
            await DiscordMessageWaiter.AssertNoMessageAfterAsync(
                channel,
                markerMessage.Id,
                message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Unknown command", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Available commands", StringComparison.OrdinalIgnoreCase),
                "command response for non-prefixed message",
                quietPeriod: TimeSpan.FromMilliseconds(1200));
        }
        finally
        {
            CommandHandlerService.UnregisterHandler(DiscordClient);
        }
    }
}
