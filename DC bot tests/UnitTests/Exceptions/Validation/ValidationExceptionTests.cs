using DC_bot.Exceptions.Validation;

namespace DC_bot_tests.UnitTests.Exceptions.Validation;

public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_ValidationKeyIsSet()
    {
        // Arrange
        const string validationKey = "user_not_in_a_voice_channel";
        const string message = "Validation failed";

        // Act
        var exception = new ValidationException(validationKey, message);

        // Assert
        Assert.Equal(validationKey, exception.ValidationKey);
    }

    [Fact]
    public void ValidationException_WithInnerException_PreservesIt()
    {
        // Arrange
        var innerException = new ArgumentNullException("field");

        // Act
        var exception = new ValidationException("TestKey", "Failed", innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
    }
}