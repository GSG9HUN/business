using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class CommandHandlerServiceQueueRoutingIntegrationTests
{
    [Fact]
    public async Task HandleCommandAsync_WithRealCommandList_RoutesQueueCommandsThroughValidationGuards()
    {
        await using var graph = CommandHandlerIntegrationFixture.CreateRealTextCommandGraph();
        var service = CommandHandlerIntegrationFixture.CreateCommandHandler(
            graph.Services,
            graph.LocalizationServiceMock.Object,
            graph.MessageFactory);

        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(
            service,
            FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!clear", isBot: false));

        graph.ResponseBuilderMock.Verify(
            response => response.SendValidationErrorAsync(
                It.IsAny<IDiscordMessage>(),
                ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
        graph.MusicQueueServiceMock.Verify(
            musicQueueService => musicQueueService.ClearQueue(It.IsAny<ulong>()),
            Times.Never);
    }
}
