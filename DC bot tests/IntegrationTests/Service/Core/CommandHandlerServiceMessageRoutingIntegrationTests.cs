using DC_bot.Interface;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class CommandHandlerServiceMessageRoutingIntegrationTests
{
    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageEvent_ExecutesRegisteredCommand()
    {
        var command = new Mock<ICommand>();
        var executed = new TaskCompletionSource<IDiscordMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");
        command.Setup(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .Callback<IDiscordMessage>(executed.SetResult)
            .Returns(Task.CompletedTask);

        var service = CommandHandlerIntegrationFixture.CreateHandlerWithCommand(command.Object);
        var args = FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!fake with args", isBot: false);

        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(service, args);

        var handledMessage = await executed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("!fake with args", handledMessage.Content);
        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageWithoutPrefix_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        var service = CommandHandlerIntegrationFixture.CreateHandlerWithCommand(command.Object);
        var args = FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("fake without prefix", isBot: false);

        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleCommandAsync_WithBotAuthorAndProductionMode_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        var service = CommandHandlerIntegrationFixture.CreateHandlerWithCommand(command.Object, isTestMode: false);
        var args = FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!fake", isBot: true);

        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }
}
