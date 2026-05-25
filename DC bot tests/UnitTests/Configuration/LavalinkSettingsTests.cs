using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

[Trait("Category", "Unit")]
public class LavalinkSettingsTests
{
    [Fact]
    public void LavalinkSettings_CanBeInitialized()
    {
        var settings = new LavalinkSettings
        {
            Hostname = "localhost",
            Port = 2333,
            Password = "password",
            Secured = false
        };

        Assert.NotNull(settings);
    }

    [Fact]
    public void LavalinkSettings_Hostname_CanBeSet()
    {
        const string hostname = "lavalink.example.com";

        var settings = new LavalinkSettings { Hostname = hostname };

        Assert.Equal(hostname, settings.Hostname);
    }

    [Fact]
    public void LavalinkSettings_Port_CanBeSet()
    {
        const int port = 2333;

        var settings = new LavalinkSettings { Port = port };

        Assert.Equal(port, settings.Port);
    }

    [Fact]
    public void LavalinkSettings_Password_CanBeSet()
    {
        const string password = "secure_password";

        var settings = new LavalinkSettings { Password = password };

        Assert.Equal(password, settings.Password);
    }

    [Fact]
    public void LavalinkSettings_Secured_CanBeSet()
    {
        var settingsSecured = new LavalinkSettings { Secured = true };
        var settingsUnsecured = new LavalinkSettings { Secured = false };

        Assert.True(settingsSecured.Secured);
        Assert.False(settingsUnsecured.Secured);
    }

    [Fact]
    public void LavalinkSettings_AllProperties_CanBeSet()
    {
        var settings = new LavalinkSettings
        {
            Hostname = "lavalink.example.com",
            Port = 443,
            Password = "password123",
            Secured = true
        };

        Assert.Equal("lavalink.example.com", settings.Hostname);
        Assert.Equal(443, settings.Port);
        Assert.Equal("password123", settings.Password);
        Assert.True(settings.Secured);
    }

    [Fact]
    public void LavalinkSettings_DifferentInstances_AreIndependent()
    {
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

        Assert.Equal("host1", settings1.Hostname);
        Assert.Equal("host2", settings2.Hostname);
        Assert.Equal(2333, settings1.Port);
        Assert.Equal(443, settings2.Port);
    }
}
