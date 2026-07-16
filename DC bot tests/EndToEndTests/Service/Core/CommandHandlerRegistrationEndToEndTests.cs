using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Service.Core;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class CommandHandlerRegistrationEndToEndTests : CommandHandlerEndToEndTestBase
{
    [Fact]
    public void RegisterHandler_ShouldRegisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = CreateFreshCommandHandler();
        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1102),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);

        freshCommandHandlerService.UnregisterHandler(mockClient);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public void UnregisterHandler_ShouldUnregisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = CreateFreshCommandHandler();
        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.UnregisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1105),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Unregistered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);

        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = CreateFreshCommandHandler();
        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.UnregisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == 1106),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("Tried to unregister handler, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);

        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public void RegisterCommandAsync_Twice_Should_Log_Already_Registered()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = CreateFreshCommandHandler();
        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.RegisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1101),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("CommandHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);

        freshCommandHandlerService.UnregisterHandler(mockClient);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }
}
