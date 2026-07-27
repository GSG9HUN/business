using Microsoft.Extensions.Logging;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionHandlerServiceRegistrationTests : ReactionHandlerServiceTestBase
{
    [Fact]
    public void RegisterHandler_WithDiscordClient_SubscribesToEvents()
    {
        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();

        reactionHandler.RegisterHandler(discordClient);

        VerifyLog(LogLevel.Information, 1202, "Registered reaction handler");

        reactionHandler.UnregisterHandler(discordClient);
    }

    [Fact]
    public void RegisterHandler_ThenUnregister_UnsubscribesFromEvents()
    {
        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();

        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.UnregisterHandler(discordClient);

        VerifyLog(LogLevel.Information, 1203, "Unregistered reaction handler");
    }

    [Fact]
    public void RegisterHandler_CalledTwice_LogsAlreadyRegisteredSecondTime()
    {
        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();

        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);

        VerifyLog(LogLevel.Information, 1201, "ReactionHandler Service is already registered");

        reactionHandler.UnregisterHandler(discordClient);
    }

    [Fact]
    public void UnregisterHandler_WithoutPreviousRegister_LogsWarning()
    {
        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();

        reactionHandler.UnregisterHandler(discordClient);

        VerifyLog(LogLevel.Warning, 1204, "Tried to unregister handlers, but it was not registered");
    }

    [Fact]
    public void RegisterUnregisterCycle_MaintainsConsistentState()
    {
        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();

        reactionHandler.RegisterHandler(discordClient);
        LoggerMock.Invocations.Clear();

        reactionHandler.UnregisterHandler(discordClient);
        VerifyLog(LogLevel.Information, 1203);

        ResetLogger();

        reactionHandler.RegisterHandler(discordClient);
        VerifyLog(LogLevel.Information, 1202);

        reactionHandler.UnregisterHandler(discordClient);
    }
}
