using System.Runtime.CompilerServices;
using DC_bot.Wrapper;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class DiscordConnectionEventLoggerTests
{
    [Theory]
    [InlineData(4014, LogLevel.Critical, "disallowed intents")]
    [InlineData(1000, LogLevel.Warning, "gateway socket closed")]
    public void LogSocketClosed_UsesGatewayCodeMeaning(int code, LogLevel level, string expected)
    {
        var args = (SocketClosedEventArgs)RuntimeHelpers.GetUninitializedObject(typeof(SocketClosedEventArgs));
        var closeCode = typeof(SocketClosedEventArgs).GetProperty(nameof(SocketClosedEventArgs.CloseCode))!;
        closeCode.SetValue(args, Convert.ChangeType(code, closeCode.PropertyType));
        var logger = new Mock<ILogger<DiscordClientEventHandler>>();
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        new DiscordConnectionEventLogger(logger.Object).LogSocketClosed(args);

        logger.Verify(l => l.Log(level, It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains(expected)
                                              && !value.ToString()!.Contains("voice disconnect")),
            It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}