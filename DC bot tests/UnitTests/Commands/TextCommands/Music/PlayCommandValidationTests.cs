using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Music;

[Trait("Category", "Unit")]
public class PlayCommandValidationTests : PlayCommandTestBase
{
    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        ArrangeValidationFailure();

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        VerifyNoPlayback();
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        ArrangeArgument("test query");
        DiscordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
        VerifyNoPlayback();
    }

    [Fact]
    public async Task ExecuteAsync_UserVoiceStateExists_ButChannelIsNull()
    {
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns((IDiscordChannel?)null);
        ArrangeArgument("test query");
        DiscordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
        VerifyNoPlayback();
    }

    [Fact]
    public async Task ExecuteAsync_UserNotProvided_URL_Or_Title()
    {
        ArrangeValidVoiceRequest(PlayCommandContentNoArgs);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(r => r.SendUsageAsync(MessageMock.Object, PlayCommandName), Times.Once);
        VerifyNoPlayback();
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedWhitespaceOnlyQuery_ShouldSendUsageAndNotStartPlayback()
    {
        ArrangeValidVoiceRequest("!play   ");

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(r => r.SendUsageAsync(MessageMock.Object, PlayCommandName), Times.Once);
        VerifyNoPlayback();
    }
}
