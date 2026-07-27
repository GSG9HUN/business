using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Queue;

[Trait("Category", "Unit")]
public class ShuffleCommandTests
{
    private const string ShuffleCommandName = "shuffle";
    private const string ShuffleCommandDescriptionValue = "Shuffle the playlist.";
    private const string ShuffleCommandResponseValue = "Queue shuffled!";
    private const string ShuffleCommandNotEnoughTracksValue = "There are not enough tracks in the queue to shuffle.";
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<ICommandHelper> _commandHelperMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly ShuffleCommand _shuffleCommand;

    private readonly Mock<IUserValidationService> _userValidationMock;

    public ShuffleCommandTests()
    {
        Mock<ILogger<ShuffleCommand>> mockLogger = new();

        _userValidationMock = new Mock<IUserValidationService>();
        _musicQueueServiceMock = new Mock<IMusicQueueService>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();

        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.ShuffleCommandDescription))
            .Returns(ShuffleCommandDescriptionValue);
        _localizationServiceMock
            .Setup(l => l.Get(It.IsAny<ulong>(), LocalizationKeys.ShuffleCommandNotEnoughTracks))
            .Returns(ShuffleCommandNotEnoughTracksValue);

        _shuffleCommand = new ShuffleCommand(
            _userValidationMock.Object,
            _musicQueueServiceMock.Object,
            mockLogger.Object,
            _responseBuilderMock.Object,
            _localizationServiceMock.Object,
            _commandHelperMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_EmptyQueue_SendsErrorMessage()
    {
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(123456789UL);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, new Mock<IDiscordMember>().Object));

        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .ReturnsAsync(new Queue<ILavaLinkTrack>());

        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendWarningAsync(_messageMock.Object, LocalizationKeys.ShuffleCommandNotEnoughTracks),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidQueue_ShufflesQueue()
    {
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(It.IsAny<ulong>());

        _userValidationMock
            .Setup(v => v.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty));

        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.ShuffleCommandResponse))
            .Returns(ShuffleCommandResponseValue);

        var originalQueue = new Queue<ILavaLinkTrack>(new List<ILavaLinkTrack>
        {
            Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>()
        });

        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .ReturnsAsync(originalQueue);

        _commandHelperMock.Setup(c => c.TryValidateUserAsync(It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, new Mock<IDiscordMember>().Object));

        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Once);

        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(),
            It.Is<Queue<ILavaLinkTrack>>(q => !q.SequenceEqual(originalQueue))), Times.Once);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object, LocalizationKeys.ShuffleCommandResponse),
            Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(ShuffleCommandName, _shuffleCommand.Name);
        Assert.Equal(ShuffleCommandDescriptionValue, _shuffleCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_SingleTrackQueue_SendsErrorMessage()
    {
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(123456789UL);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, new Mock<IDiscordMember>().Object));

        var singleTrackQueue = new Queue<ILavaLinkTrack>(new List<ILavaLinkTrack>
        {
            Mock.Of<ILavaLinkTrack>()
        });

        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .ReturnsAsync(singleTrackQueue);

        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Never);
        _responseBuilderMock.Verify(r => r.SendWarningAsync(_messageMock.Object, LocalizationKeys.ShuffleCommandNotEnoughTracks),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFails_ShouldNotShuffle()
    {
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);

        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        _musicQueueServiceMock.Verify(m => m.GetQueue(It.IsAny<ulong>()), Times.Never);
        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QueueWithIdenticalTrackReferences_UsesRetryPathAndKeepsSequenceEqual()
    {
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(123456789UL);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, new Mock<IDiscordMember>().Object));

        var sameTrack = Mock.Of<ILavaLinkTrack>();
        var originalQueue = new Queue<ILavaLinkTrack>([sameTrack, sameTrack, sameTrack, sameTrack]);

        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .ReturnsAsync(originalQueue);

        _musicQueueServiceMock
            .Setup(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()))
            .Returns(Task.CompletedTask);

        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        _musicQueueServiceMock.Verify(m => m.SetQueue(
                It.IsAny<ulong>(),
                It.Is<Queue<ILavaLinkTrack>>(q => q.SequenceEqual(originalQueue))),
            Times.Once);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object, LocalizationKeys.ShuffleCommandResponse),
            Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendErrorAsync(_messageMock.Object, It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }
}
