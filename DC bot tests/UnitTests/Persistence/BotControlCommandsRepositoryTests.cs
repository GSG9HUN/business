using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Repositories.BotControl;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class BotControlCommandsRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"BotControl_{Guid.NewGuid()}");

    [Fact]
    public async Task EnqueueAsync_WithPayload_PersistsPendingCommand()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);
        const string payloadJson = "{\"query\":\"never gonna give you up\"}";

        var command = await repository.EnqueueAsync(42UL, 99UL, "play", payloadJson, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(command.CommandId));
        Assert.Equal(42UL, command.GuildId);
        Assert.Equal(99UL, command.UserId);
        Assert.Equal("play", command.Type);
        Assert.Equal(BotControlCommandState.Pending, command.State);
        Assert.Equal(payloadJson, command.PayloadJson);
        Assert.Null(command.ErrorMessage);
        Assert.True(command.CreatedAtUtc > DateTimeOffset.MinValue);
        Assert.Null(command.ClaimedAtUtc);
        Assert.Null(command.CompletedAtUtc);

        await using var dbContext = factory.CreateDbContext();
        var saved = Assert.Single(dbContext.BotControlCommands);
        Assert.Equal(payloadJson, saved.PayloadJson);
    }

    [Fact]
    public async Task GetByCommandIdAsync_WhenCommandExists_ReturnsCommandMetadata()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);
        var command = await repository.EnqueueAsync(42UL, 99UL, "pause", CancellationToken.None);

        var result = await repository.GetByCommandIdAsync(command.CommandId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(command.CommandId, result.CommandId);
        Assert.Equal(42UL, result.GuildId);
        Assert.Equal(99UL, result.UserId);
        Assert.Equal("pause", result.Type);
        Assert.Equal(BotControlCommandState.Pending, result.State);
        Assert.Null(result.ErrorMessage);
        Assert.True(result.CreatedAtUtc > DateTimeOffset.MinValue);
        Assert.Null(result.ClaimedAtUtc);
        Assert.Null(result.CompletedAtUtc);
    }

    [Fact]
    public async Task GetByCommandIdAsync_WhenCommandDoesNotExist_ReturnsNull()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);

        var result = await repository.GetByCommandIdAsync("missing-command-id", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ClaimNextPendingAsync_MarksOldestPendingCommandAsStarted()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);
        var oldest = await repository.EnqueueAsync(42UL, 99UL, "pause", CancellationToken.None);
        _ = await repository.EnqueueAsync(42UL, 99UL, "skip", CancellationToken.None);

        var claimed = await repository.ClaimNextPendingAsync(CancellationToken.None);

        Assert.NotNull(claimed);
        Assert.Equal(oldest.CommandId, claimed.CommandId);
        Assert.Equal(BotControlCommandState.Started, claimed.State);

        await using var dbContext = factory.CreateDbContext();
        var saved = dbContext.BotControlCommands.Single(command => command.CommandId == oldest.CommandId);
        Assert.Equal(BotControlCommandState.Started, saved.Status);
        Assert.NotNull(saved.ClaimedAtUtc);
    }

    [Fact]
    public async Task MarkDoneAsync_MarksCommandAsDone()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);
        var command = await repository.EnqueueAsync(42UL, 99UL, "pause", CancellationToken.None);

        await repository.MarkDoneAsync(command.CommandId, CancellationToken.None);

        await using var dbContext = factory.CreateDbContext();
        var saved = Assert.Single(dbContext.BotControlCommands);
        Assert.Equal(BotControlCommandState.Done, saved.Status);
        Assert.Null(saved.ErrorMessage);
        Assert.NotNull(saved.CompletedAtUtc);
    }

    [Fact]
    public async Task MarkFailedAsync_MarksCommandAsFailedWithError()
    {
        var factory = CreateFactory();
        var repository = new BotControlCommandsRepository(factory);
        var command = await repository.EnqueueAsync(42UL, 99UL, "pause", CancellationToken.None);

        await repository.MarkFailedAsync(command.CommandId, "boom", CancellationToken.None);

        var result = await repository.GetByCommandIdAsync(command.CommandId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(BotControlCommandState.Failed, result.State);
        Assert.Equal("boom", result.ErrorMessage);
        Assert.NotNull(result.CompletedAtUtc);
    }
}
