using System.Text.Json;
using DC_bot.BotControl;
using DC_bot.Configuration;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Service.BotControl;
using DC_bot.Service.Music;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.BotControl;

[Trait("Category", "Unit")]
public sealed class BotControlPlayDispatcherTests
{
    [Theory]
    [InlineData(null, "InvalidPayload")]
    [InlineData("{}", "EmptyQuery")]
    [InlineData("{\"Query\":\"file:///secret\"}", "InvalidPlayInput")]
    public async Task InvalidPlay_IsTerminalFailureWithoutPlayback(string? payload, string error)
    {
        var lava = new Mock<ILavaLinkService>(MockBehavior.Strict);
        var context = new Mock<IBotControlContextResolver>(MockBehavior.Strict);
        var result = await Create(lava, context).DispatchAsync(Command(payload), default);
        Assert.False(result.Success);
        using var json = JsonDocument.Parse(result.ResultJson);
        Assert.Equal(error, json.RootElement.GetProperty("errorCode").GetString());
        lava.VerifyNoOtherCalls();
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingVoiceChannel_IsTerminalFailureWithoutPlayback()
    {
        var lava = new Mock<ILavaLinkService>(MockBehavior.Strict);
        var context = new Mock<IBotControlContextResolver>(MockBehavior.Strict);
        var command = Command(JsonSerializer.Serialize(new QueueEnqueueCommandPayload("https://www.youtube.com/watch?v=abc", null, "User")));
        context.Setup(c => c.ResolveAsync(command, It.IsAny<BotControlCommandChannelContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BotControlExecutionContext(123, 42, null, null, false));
        var result = await Create(lava, context).DispatchAsync(command, default);
        Assert.False(result.Success);
        using var json = JsonDocument.Parse(result.ResultJson);
        Assert.Equal("UserNotInVoiceChannel", json.RootElement.GetProperty("errorCode").GetString());
        lava.VerifyNoOtherCalls();
    }

    private static BotControlCommandDispatcher Create(Mock<ILavaLinkService> lava, Mock<IBotControlContextResolver> context) => new(
        Mock.Of<IMusicQueueService>(), new BotControlResultFactory(), context.Object, lava.Object,
        Mock.Of<IRepeatService>(), Mock.Of<ICurrentTrackService>(), null!,
        new TrackSearchResolverService(Options.Create(new SearchResolverOptions())));

    private static BotControlCommandRecord Command(string? payload) => new(
        "command-1", 123, 42, BotControlCommandTypes.Play, BotControlCommandState.Pending, payload, null,
        DateTimeOffset.UtcNow, null, null, null);
}
