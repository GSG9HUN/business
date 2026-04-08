using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
/*
namespace DC_bot_tests.UnitTests.Helper
{
    public class DiscordMessageWrapperFactoryTests
    {
        [Fact]
        public void Create_ReturnsWrapperWithCorrectProperties()
        {
            // Arrange
            var messageId = 123456789012345678UL;
            var content = "Test message content";
            var author = new DiscordUser { Username = "TestUser" };
            var channel = new DiscordChannel();
            var creationTimestamp = DateTimeOffset.UtcNow;
            var embeds = new List<DiscordEmbed> { new DiscordEmbedBuilder().WithTitle("Test").Build() };

            var messageMock = new Mock<DiscordMessage>();
            messageMock.SetupGet(m => m.Id).Returns(messageId);
            messageMock.SetupGet(m => m.Content).Returns(content);
            messageMock.SetupGet(m => m.Author).Returns(author);
            messageMock.SetupGet(m => m.Channel).Returns(channel);
            messageMock.SetupGet(m => m.CreationTimestamp).Returns(creationTimestamp);
            messageMock.SetupGet(m => m.Embeds).Returns(embeds);
            messageMock.Setup(m => m.RespondAsync(It.IsAny<string>(), null, null, null, null, null, null, default)).Returns(Task.FromResult<DiscordMessage>(null));
            messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.FromResult<DiscordMessage>(null));

            var loggerMock = new Mock<ILogger<DiscordMessageWrapper>>();

            // Act
            var wrapper = DiscordMessageWrapperFactory.Create(messageMock.Object, channel, author, loggerMock.Object);

            // Assert
            Assert.Equal(messageId, wrapper.Id);
            Assert.Equal(content, wrapper.Content);
            Assert.Equal(creationTimestamp, wrapper.CreationTimestamp);
            Assert.Equal(embeds, wrapper.Embeds);
            Assert.Equal(author.Username, wrapper.Author.Username);
            Assert.Equal(channel, wrapper.Channel.Unwrap());
        }

        [Fact]
        public async Task Create_DelegatesRespondAsyncAndModifyAsync()
        {
            // Arrange
            var messageId = 123456789012345678UL;
            var content = "Test message content";
            var author = new DiscordUser { Username = "TestUser" };
            var channel = new DiscordChannel();
            var creationTimestamp = DateTimeOffset.UtcNow;
            var embeds = new List<DiscordEmbed>();

            var messageMock = new Mock<DiscordMessage>();
            messageMock.SetupGet(m => m.Id).Returns(messageId);
            messageMock.SetupGet(m => m.Content).Returns(content);
            messageMock.SetupGet(m => m.Author).Returns(author);
            messageMock.SetupGet(m => m.Channel).Returns(channel);
            messageMock.SetupGet(m => m.CreationTimestamp).Returns(creationTimestamp);
            messageMock.SetupGet(m => m.Embeds).Returns(embeds);

            var respondCalled = false;
            var modifyCalled = false;

            messageMock.Setup(m => m.RespondAsync(It.IsAny<string>(), null, null, null, null, null, null, default))
                .Callback(() => respondCalled = true)
                .Returns(Task.FromResult<DiscordMessage>(null));
            messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
                .Callback(() => modifyCalled = true)
                .Returns(Task.FromResult<DiscordMessage>(null));

            var loggerMock = new Mock<ILogger<DiscordMessageWrapper>>();

            var wrapper = DiscordMessageWrapperFactory.Create(messageMock.Object, channel, author, loggerMock.Object);

            // Act
            await wrapper.RespondAsync("test");
            await wrapper.ModifyAsync(new DiscordMessageBuilder());

            // Assert
            Assert.True(respondCalled);
            Assert.True(modifyCalled);
        }
    }
}

*/