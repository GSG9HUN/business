using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

[Trait("Category", "Unit")]
public class LocalizationServiceDirectoryTests : LocalizationServiceTestBase
{
    [Fact]
    public void Constructor_CreatesLocalizationDirectory_WhenDirectoryDoesNotExist()
    {
        SetupLocalizationDirectory(exists: false);

        _ = CreateService();

        FileSystemMock.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotCreateDirectory_WhenDirectoryExists()
    {
        SetupLocalizationDirectory();

        _ = CreateService();

        FileSystemMock.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
    }
}
