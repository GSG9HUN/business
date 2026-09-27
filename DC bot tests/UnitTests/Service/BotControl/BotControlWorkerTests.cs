using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Service.BotControl;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.BotControl;

[Trait("Category", "Unit")]
public class BotControlWorkerTests
{
    private static readonly BotControlCommandRecord Command = new(
        "command-1",
        123UL,
        456UL,
        "pause",
        BotControlCommandState.Pending,
        null,
        null,
        DateTimeOffset.UtcNow,
        null,
        null,
        null);

    [Fact]
    public async Task RunAsync_EnsuresNotifierIsListeningBeforeFirstDrain()
    {
        var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        var sequence = 0;
        var ensureOrder = 0;
        var claimOrder = 0;

        context.Notifier
            .Setup(notifier => notifier.EnsureListeningAsync(It.IsAny<CancellationToken>()))
            .Callback(() => ensureOrder = ++sequence)
            .Returns(Task.CompletedTask);
        context.Repository
            .Setup(repository => repository.ClaimNextPendingAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                claimOrder = ++sequence;
                cancellation.Cancel();
            })
            .ReturnsAsync((BotControlCommandRecord?)null);

        await context.Worker.RunAsync(cancellation.Token);

        Assert.True(ensureOrder > 0);
        Assert.True(claimOrder > 0);
        Assert.True(ensureOrder < claimOrder);
    }

    [Fact]
    public async Task RunAsync_WhenStartedCommandsExist_MarksThemFailedAsInterrupted()
    {
        var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        var startedCommand = Command with
        {
            State = BotControlCommandState.Started,
            ClaimedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        context.Repository
            .Setup(repository => repository.GetStartedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([startedCommand]);
        context.Notifier
            .Setup(notifier => notifier.EnsureListeningAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cancellation.Cancel())
            .Returns(Task.CompletedTask);

        await context.Worker.RunAsync(cancellation.Token);

        context.Repository.Verify(
            repository => repository.MarkFailedAsync(
                startedCommand.CommandId,
                "Command was interrupted before completion.",
                It.Is<string>(json =>
                    json.Contains("\"success\":false") &&
                    json.Contains("\"errorCode\":\"Interrupted\"")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenDispatcherThrows_PersistsGenericFailureWithoutExceptionText()
    {
        var context = CreateContext();
        using var cancellation = new CancellationTokenSource();

        context.Repository
            .SetupSequence(repository => repository.ClaimNextPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Command)
            .ReturnsAsync((BotControlCommandRecord?)null);
        context.Dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(Command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("secret sql host detail"));
        context.Notifier
            .Setup(notifier => notifier.WaitForCommandAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cancellation.Cancel())
            .Returns(Task.CompletedTask);

        await context.Worker.RunAsync(cancellation.Token);

        context.Repository.Verify(
            repository => repository.MarkFailedAsync(
                Command.CommandId,
                "Command failed.",
                It.Is<string>(json =>
                    json.Contains("\"message\":\"Command failed.\"") &&
                    json.Contains("\"errorCode\":\"UnexpectedError\"") &&
                    !json.Contains("secret sql host detail")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenDiscordSendFailsStillMarksSuccessfulCommandDone()
    {
        var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        var result = new BotControlCommandResult(
            true,
            "Done.",
            """{"success":true,"discordNotificationSent":false}""",
            Command.GuildId,
            Command.UserId,
            null,
            null,
            ShouldNotifyDiscord: true);

        context.Repository
            .SetupSequence(repository => repository.ClaimNextPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Command)
            .ReturnsAsync((BotControlCommandRecord?)null);
        context.Dispatcher
            .Setup(dispatcher => dispatcher.DispatchAsync(Command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        context.DiscordResponse
            .Setup(service => service.TrySendAsync(result, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.Notifier
            .Setup(notifier => notifier.WaitForCommandAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cancellation.Cancel())
            .Returns(Task.CompletedTask);

        await context.Worker.RunAsync(cancellation.Token);

        context.Repository.Verify(
            repository => repository.MarkDoneAsync(
                Command.CommandId,
                It.Is<string>(json => json.Contains("\"discordNotificationSent\":false")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        context.Repository.Verify(
            repository => repository.MarkFailedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static TestContext CreateContext()
    {
        var repository = new Mock<IBotControlCommandsRepository>();
        var notifier = new Mock<IBotControlCommandNotifier>();
        var dispatcher = new Mock<IBotControlCommandDispatcher>();
        var discordResponse = new Mock<IBotControlDiscordResponseService>();
        var resultFactory = new BotControlResultFactory();

        repository
            .Setup(repo => repo.GetStartedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        repository
            .Setup(repo => repo.ClaimNextPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotControlCommandRecord?)null);
        notifier
            .Setup(n => n.EnsureListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        notifier
            .Setup(n => n.WaitForCommandAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        discordResponse
            .Setup(service => service.TrySendAsync(It.IsAny<BotControlCommandResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var worker = new BotControlWorker(
            repository.Object,
            notifier.Object,
            dispatcher.Object,
            discordResponse.Object,
            resultFactory,
            Mock.Of<ILogger<BotControlWorker>>());

        return new TestContext(repository, notifier, dispatcher, discordResponse, worker);
    }

    private sealed record TestContext(
        Mock<IBotControlCommandsRepository> Repository,
        Mock<IBotControlCommandNotifier> Notifier,
        Mock<IBotControlCommandDispatcher> Dispatcher,
        Mock<IBotControlDiscordResponseService> DiscordResponse,
        BotControlWorker Worker);
}
