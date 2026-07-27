using DC_bot.Constants;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class CommandHandlerServiceUtilityRoutingIntegrationTests
{
    [Fact]
    public async Task HandleCommandAsync_WithRealCommandList_RoutesUtilityCommandsThroughFakeDiscordMessages()
    {
        await using var graph = CommandHandlerIntegrationFixture.CreateRealTextCommandGraph();
        var service = CommandHandlerIntegrationFixture.CreateCommandHandler(
            graph.Services,
            graph.LocalizationServiceMock.Object,
            graph.MessageFactory);

        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(
            service,
            FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!ping", isBot: false));
        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(
            service,
            FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!help", isBot: false));
        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(
            service,
            FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!language hu", isBot: false));
        await CommandHandlerIntegrationFixture.InvokeHandleCommandAsync(
            service,
            FakeDiscordMessageBuilder.CreateMessageCreateEventArgs("!tag", isBot: false));

        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.PingCommandResponse),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.HelpCommandResponse,
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0].ToString()!.Contains("ping", StringComparison.Ordinal) &&
                    args[0].ToString()!.Contains("play", StringComparison.Ordinal))),
            Times.Once);
        graph.LocalizationServiceMock.Verify(
            localization => localization.SaveLanguage(456UL, "hu"),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.LanguageCommandResponse),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendUsageAsync(It.IsAny<IDiscordMessage>(), "tag"),
            Times.Once);
    }
}
