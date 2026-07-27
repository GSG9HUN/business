using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionHandlerServiceDispatchTests : ReactionHandlerServiceTestBase
{
    public static IEnumerable<object[]> AddedReactionCases()
    {
        yield return ["⏸️", "Pause"];
        yield return ["▶️", "Resume"];
        yield return ["⏭️", "Skip"];
        yield return ["🔁", "RepeatOn"];
    }

    public static IEnumerable<object[]> RemovedReactionCases()
    {
        yield return ["⏸️", "Resume"];
        yield return ["▶️", "Pause"];
        yield return ["⏭️", "Skip"];
        yield return ["🔁", "RepeatOff"];
    }

    [Theory]
    [MemberData(nameof(AddedReactionCases))]
    public async Task HandleReactionAddedAsync_WhenSupportedEmoji_ExecutesExpectedAction(
        string emoji,
        string expectedAction)
    {
        var target = CreateReactionTarget();
        SetupSuccessfulPlaybackOperations();
        var reactionHandler = CreateHandler();

        await reactionHandler.HandleReactionAddedAsync(emoji, target.Message, target.Member);

        LavaLinkServiceMock.Verify(x => x.PauseAsync(target.Message, target.Member),
            expectedAction == "Pause" ? Times.Once() : Times.Never());
        LavaLinkServiceMock.Verify(x => x.ResumeAsync(target.Message, target.Member),
            expectedAction == "Resume" ? Times.Once() : Times.Never());
        LavaLinkServiceMock.Verify(x => x.SkipAsync(target.Message, target.Member),
            expectedAction == "Skip" ? Times.Once() : Times.Never());
        target.MessageMock.Verify(x => x.RespondAsync("Repeat on"),
            expectedAction == "RepeatOn" ? Times.Once() : Times.Never());
    }

    [Theory]
    [MemberData(nameof(RemovedReactionCases))]
    public async Task HandleReactionRemovedAsync_WhenSupportedEmoji_ExecutesExpectedAction(
        string emoji,
        string expectedAction)
    {
        var target = CreateReactionTarget();
        SetupSuccessfulPlaybackOperations();
        var reactionHandler = CreateHandler();

        await reactionHandler.HandleReactionRemovedAsync(emoji, target.Message, target.Member);

        LavaLinkServiceMock.Verify(x => x.PauseAsync(target.Message, target.Member),
            expectedAction == "Pause" ? Times.Once() : Times.Never());
        LavaLinkServiceMock.Verify(x => x.ResumeAsync(target.Message, target.Member),
            expectedAction == "Resume" ? Times.Once() : Times.Never());
        LavaLinkServiceMock.Verify(x => x.SkipAsync(target.Message, target.Member),
            expectedAction == "Skip" ? Times.Once() : Times.Never());
        target.MessageMock.Verify(x => x.RespondAsync("Repeat off"),
            expectedAction == "RepeatOff" ? Times.Once() : Times.Never());
    }
}
