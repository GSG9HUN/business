using DC_bot.Exceptions.Validation;

namespace DC_bot_tests.UnitTests.Exceptions.Validation;

[Trait("Category", "Unit")]
public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_ValidationKeyIsSet()
    {
        const string validationKey = "user_not_in_a_voice_channel";
        const string message = "Validation failed";

        var exception = new ValidationException(validationKey, message);

        Assert.Equal(validationKey, exception.ValidationKey);
    }

    [Fact]
    public void ValidationException_WithInnerException_PreservesIt()
    {
        var innerException = new ArgumentNullException("field");

        var exception = new ValidationException("TestKey", "Failed", innerException);

        Assert.Equal(innerException, exception.InnerException);
    }
}
