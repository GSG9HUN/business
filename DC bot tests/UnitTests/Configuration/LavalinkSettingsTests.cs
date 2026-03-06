using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

public class LavalinkSettingsTests
{
    [Fact]
    public void LavalinkSettings_CanBeInitialized()
    {
        // Arrange & Act
        var settings = new LavalinkSettings 
        { 
            Hostname = "localhost",
            Port = 2333,
            Password = "password",
            Secured = false
        };

        // Assert
        Assert.NotNull(settings);
    }

    [Fact]
    public void LavalinkSettings_Hostname_CanBeSet()
    {
        // Arrange
        const string hostname = "lavalink.example.com";

        // Act
        var settings = new LavalinkSettings { Hostname = hostname };

        // Assert
        Assert.Equal(hostname, settings.Hostname);
    }

    [Fact]
    public void LavalinkSettings_Port_CanBeSet()
    {
        // Arrange
        const int port = 2333;

        // Act
        var settings = new LavalinkSettings { Port = port };

        // Assert
        Assert.Equal(port, settings.Port);
    }

    [Fact]
    public void LavalinkSettings_Password_CanBeSet()
    {
        // Arrange
        const string password = "secure_password";

        // Act
        var settings = new LavalinkSettings { Password = password };

        // Assert
        Assert.Equal(password, settings.Password);
    }

    [Fact]
    public void LavalinkSettings_Secured_CanBeSet()
    {
        // Arrange & Act
        var settingsSecured = new LavalinkSettings { Secured = true };
        var settingsUnsecured = new LavalinkSettings { Secured = false };

        // Assert
        Assert.True(settingsSecured.Secured);
        Assert.False(settingsUnsecured.Secured);
    }

    [Fact]
    public void LavalinkSettings_AllProperties_CanBeSet()
    {
        // Arrange & Act
        var settings = new LavalinkSettings 
        { 
            Hostname = "lavalink.example.com",
            Port = 443,
            Password = "password123",
            Secured = true
        };

        // Assert
        Assert.Equal("lavalink.example.com", settings.Hostname);
        Assert.Equal(443, settings.Port);
        Assert.Equal("password123", settings.Password);
        Assert.True(settings.Secured);
    }

    [Fact]
    public void LavalinkSettings_DifferentInstances_AreIndependent()
    {
        // Arrange & Act
        var settings1 = new LavalinkSettings 
        { 
            Hostname = "host1", 
            Port = 2333 
        };
        var settings2 = new LavalinkSettings 
        { 
            Hostname = "host2", 
            Port = 443 
        };

        // Assert
        Assert.Equal("host1", settings1.Hostname);
        Assert.Equal("host2", settings2.Hostname);
        Assert.Equal(2333, settings1.Port);
        Assert.Equal(443, settings2.Port);
    }
}