using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Service.SlashCommands;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.SlashCommands;

[Trait("Category", "Unit")]
public class SlashCommandExecutorTests
{
    private const ulong GuildId = 123UL;

    private readonly Mock<ICommand> _commandMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly SlashCommandExecutor _executor;

    public SlashCommandExecutorTests()
    {
        _commandMock.SetupGet(command => command.Name).Returns("ping");
        _commandMock.Setup(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .Returns(Task.CompletedTask);

        _localizationServiceMock
            .Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(Format);
        _localizationServiceMock
            .Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => Format(key, args));

        _executor = new SlashCommandExecutor(
            [_commandMock.Object],
            Mock.Of<ILogger<SlashCommandExecutor>>(),
            _localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGuildIsRequiredAndMissing_RespondsWithGuildOnlyMessage()
    {
        var context = new TestSlashInteractionContext(guild: null);

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest(
            "ping",
            context,
            RequireGuild: true));

        Assert.Equal("Guild only.", Assert.Single(context.TextResponses));
        _commandMock.Verify(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandIsNotRegistered_RespondsWithLocalizedMessage()
    {
        var context = CreateGuildContext();

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest("missing", context));

        Assert.Equal("Command 'missing' is not registered.", Assert.Single(context.TextResponses));
        _commandMock.Verify(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeferredCommandDoesNotRespond_SendsAcceptedFallback()
    {
        var context = CreateGuildContext();

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest(
            "ping",
            context,
            Defer: true,
            EnsureDeferredResponse: true));

        Assert.True(context.IsDeferred);
        Assert.Equal("Accepted.", Assert.Single(context.TextResponses));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandThrowsBotException_RespondsWithDomainMessage()
    {
        var context = CreateGuildContext();
        _commandMock.Setup(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .ThrowsAsync(new TestBotException("Domain failure."));

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest("ping", context));

        Assert.Equal("Domain failure.", Assert.Single(context.TextResponses));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandThrowsUnexpectedException_RespondsWithLocalizedFallback()
    {
        var context = CreateGuildContext();
        _commandMock.Setup(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest("ping", context));

        Assert.Equal("Unexpected slash error.", Assert.Single(context.TextResponses));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandAlreadyResponded_DoesNotSendDeferredFallback()
    {
        var context = CreateGuildContext();
        _commandMock.Setup(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .Returns<IDiscordMessage>(message => message.RespondAsync("Command response."));

        await _executor.ExecuteAsync(new SlashCommandExecutionRequest(
            "ping",
            context,
            Defer: true,
            EnsureDeferredResponse: true));

        Assert.Equal(["Command response."], context.TextResponses);
    }

    private static TestSlashInteractionContext CreateGuildContext()
    {
        var guild = new Mock<IDiscordGuild>();
        guild.SetupGet(x => x.Id).Returns(GuildId);
        guild.SetupGet(x => x.Name).Returns("SlashGuild");

        return new TestSlashInteractionContext(guild: guild.Object);
    }

    private static string Format(string key, object[] args)
    {
        return key switch
        {
            LocalizationKeys.SlashCommandGuildOnly => "Guild only.",
            LocalizationKeys.SlashCommandNotRegistered => $"Command '{args[0]}' is not registered.",
            LocalizationKeys.SlashCommandDeferredAccepted => "Accepted.",
            LocalizationKeys.SlashCommandUnexpectedError => "Unexpected slash error.",
            _ => key
        };
    }

    private sealed class TestBotException(string message) : BotException(message);
}
