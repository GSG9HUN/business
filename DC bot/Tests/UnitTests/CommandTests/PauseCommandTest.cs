using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class PauseCommandTests
{
    private readonly LavaLinkService _mockLavaLinkService;
    private readonly Mock<ILogger<PauseCommand>> _mockLogger;
    private readonly Mock<ILogger<LavaLinkService>> _mockLoggerLavalink;
    private readonly Mock<IDiscordMessageWrapper> _mockMessage;
    private readonly Mock<MusicQueueService> _mockMusicQueueService;
    private readonly PauseCommand _pauseCommand;

    /* public PauseCommandTests()
     {
         // Mock Logger
         _mockLogger = new Mock<ILogger<PauseCommand>>();

         _mockMusicQueueService = new Mock<MusicQueueService>();
         // Mock LavaLinkService
         _mockLavaLinkService = new LavaLinkService(_mockMusicQueueService.Object, _mockLoggerLavalink.Object);

         // Mock IDiscordMessageWrapper
         _mockMessage = new Mock<IDiscordMessageWrapper>();
         var mockChannel = new Mock<IDiscordChannel>();
         var mockGuild = new Mock<IDiscordGuild>();
         var mockMember = new Mock<IDiscordMember>();
         var mockVoiceState = new Mock<IDiscordVoiceState>();

         // Beállítjuk a mockolt channel és member tulajdonságokat
         _mockMessage.Setup(m => m.Channel).Returns(mockChannel.Object);
         mockChannel.Setup(c => c.Guild).Returns(mockGuild.Object);
         mockGuild.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(mockMember.Object);

         // Command inicializálása
         _pauseCommand = new PauseCommand(_mockLavaLinkService, _mockLogger.Object);
     }

     [Fact]
     public async Task ExecuteAsync_ShouldSendMessage_WhenUserIsNotInVoiceChannel()
     {
         // Arrange: A felhasználó nincs voice csatornában
         var mockVoiceState = new Mock<IDiscordVoiceState>();
         mockVoiceState.Setup(v => v.Channel).Returns((IDiscordChannel)null);
         var mockMember = new Mock<IDiscordMember>();
         mockMember.Setup(m => m.VoiceState).Returns(mockVoiceState.Object);
         _mockMessage.Setup(m => m.Channel.Guild.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(mockMember.Object);

         // Act
         await _pauseCommand.ExecuteAsync(_mockMessage.Object);

         // Assert: A "You must be in a voice channel!" üzenet küldése
         _mockMessage.Verify(m => m.RespondAsync("You must be in a voice channel.!"), Times.Once);
     }

     [Fact]
     public async Task ExecuteAsync_ShouldPauseMusic_WhenUserIsInVoiceChannel()
     {
         // Arrange: A felhasználó voice csatornában van
         var mockVoiceChannel = new Mock<IDiscordChannel>();
         var mockVoiceState = new Mock<IDiscordVoiceState>();
         mockVoiceState.Setup(v => v.Channel).Returns(mockVoiceChannel.Object);
         var mockMember = new Mock<IDiscordMember>();
         mockMember.Setup(m => m.VoiceState).Returns(mockVoiceState.Object);
         _mockMessage.Setup(m => m.Channel.Guild.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(mockMember.Object);

         // Act
         await _pauseCommand.ExecuteAsync(_mockMessage.Object);

         // Assert: A PauseAsync hívódik meg
         //_mockLavaLinkService.Verify(l => l.PauseAsync(It.IsAny<IDiscordChannel>()), Times.Once);
         _mockMessage.Verify(m => m.RespondAsync(It.IsAny<string>()), Times.Never); // Nem küldünk üzenetet
     }

     [Fact]
     public async Task ExecuteAsync_ShouldNotPauseIfMemberIsBot()
     {
         // Arrange: A felhasználó bot
         var mockMember = new Mock<IDiscordMember>();
         mockMember.Setup(m => m.IsBot).Returns(true);
         _mockMessage.Setup(m => m.Channel.Guild.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(mockMember.Object);

         // Act
         await _pauseCommand.ExecuteAsync(_mockMessage.Object);

         // Assert: A PauseAsync nem hívódik meg
         //_mockLavaLinkService.Verify(l => l.PauseAsync(It.IsAny<IDiscordChannel>()), Times.Never);
         _mockMessage.Verify(m => m.RespondAsync(It.IsAny<string>()), Times.Never); // Nem küldünk üzenetet
     }*/
}