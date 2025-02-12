using System.Reflection;
using DC_bot.Commands;
using DC_bot.Services;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot_test.Commands;

[TestSubject(typeof(PauseCommand))]
public class PauseCommandTest
{
    [Fact]
    public async Task ExecuteAsync_ShouldInformUser_WhenNotInVoiceChannel()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DISCORD_TOKEN", "test-token");

        var discordLogger = new Mock<ILogger<SingletonDiscordClient>>();
        SingletonDiscordClient.InitializeLogger(discordLogger.Object);

        var lavaLinkLogger = new Mock<ILogger<LavaLinkService>>();
        var pauseLoggerMock = new Mock<ILogger<PauseCommand>>();

        // Valódi LavaLinkService példány
        var lavaLinkService = new LavaLinkService(lavaLinkLogger.Object);
        var pauseCommand = new PauseCommand(lavaLinkService, pauseLoggerMock.Object);

        // Valódi DiscordChannel és DiscordMessage inicializálása
        var guild = (DiscordGuild)Activator.CreateInstance(typeof(DiscordGuild), true);
        var author = (DiscordUser)Activator.CreateInstance(typeof(DiscordUser), true);
        var channel = (DiscordChannel)Activator.CreateInstance(typeof(DiscordChannel), true);
        typeof(DiscordChannel).GetProperty("Guild", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(channel, (DiscordGuild)Activator.CreateInstance(typeof(DiscordGuild), true));

        var message = (DiscordMessage)Activator.CreateInstance(typeof(DiscordMessage), true);
        typeof(DiscordMessage).GetProperty("Channel", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(message, channel);
        
        // Act
        await pauseCommand.ExecuteAsync(new MessageWrapper(message));

        // Assert
        pauseLoggerMock.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString() == "User not in a voice channel."),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallPauseAsync_WhenUserInVoiceChannel()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DISCORD_TOKEN", "test-token");

        var pauseLoggerMock = new Mock<ILogger<PauseCommand>>();
        var lavalinkLoggerMock = new Mock<ILogger<LavaLinkService>>();
        var lavaLinkService = new LavaLinkService(lavalinkLoggerMock.Object); // Valódi példány mockolt paraméterrel
        var discordLogger = new Mock<ILogger<SingletonDiscordClient>>();
        SingletonDiscordClient.InitializeLogger(discordLogger.Object);
        var lavaLinkServiceMock = Mock.Get(lavaLinkService); // Mockolja a valódi példányt
        var pauseCommand = new PauseCommand(lavaLinkServiceMock.Object, pauseLoggerMock.Object);

        // Mock DiscordMessage
        var messageMock = new Mock<DiscordMessage>();
        var guildMock = new Mock<DiscordGuild>();
        var channelMock = new Mock<DiscordChannel>();
        var memberMock = new Mock<DiscordMember>();
        var voiceStateMock = new Mock<DiscordVoiceState>();
        var voiceChannelMock = new Mock<DiscordChannel>();

        messageMock.Setup(m => m.Channel).Returns(channelMock.Object);
        messageMock.Setup(m => m.Channel.Guild).Returns(guildMock.Object);
        guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>(), false)).ReturnsAsync(memberMock.Object);

        // Felhasználó voice channelben van
        memberMock.Setup(m => m.VoiceState).Returns(voiceStateMock.Object);
        voiceStateMock.Setup(v => v.Channel).Returns(voiceChannelMock.Object);

        channelMock
            .Setup(c => c.SendMessageAsync(
                It.IsAny<string>()))
            .ReturnsAsync(messageMock.Object);

        // Act
        await pauseCommand.ExecuteAsync(messageMock.Object);

        // Assert
        lavaLinkServiceMock.Verify(l => l.PauseAsync(It.IsAny<DiscordChannel>()), Times.Once);
        pauseLoggerMock.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString() == "Pause command executed!"),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}