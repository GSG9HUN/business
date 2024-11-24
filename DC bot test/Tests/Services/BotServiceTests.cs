using DC_bot.Services;
using Discord;
using DotNetEnv;
using Moq;
using Xunit;
using System.Threading.Tasks;
using IDiscordClient = DC_bot.Interface.IDiscordClient;

namespace DC_bot_test.Tests.Services;

public class BotServiceTests
{
    [Fact]
    public async Task StartAsync_AttachesEventHandlersAndStartsClient()
    {
        // Arrange
        var mockClient = new Mock<IDiscordClient>();
        var mockCommandHandler = new Mock<CommandHandler>();
        Env.Load();
            
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        
        mockClient.Setup(c => c.LoginAsync(TokenType.Bot, It.IsAny<string>())).Returns(Task.CompletedTask);
        mockClient.Setup(c => c.StartAsync()).Returns(Task.CompletedTask);

        var botService = new BotService(mockClient.Object, mockCommandHandler.Object);

        // Act
        await botService.StartAsync(token, true);

        // Assert
        mockClient.Verify(
            c => c.LoginAsync(TokenType.Bot,
                token), Times.Once);
        mockClient.Verify(c => c.StartAsync(), Times.Once);
    }
}