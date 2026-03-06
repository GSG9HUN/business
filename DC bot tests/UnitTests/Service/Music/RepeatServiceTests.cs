using DC_bot.Service.Music.MusicServices;

namespace DC_bot_tests.UnitTests.Service.Music;

public class RepeatServiceTests
{
    [Fact]
    public void Init_DefaultsToFalse()
    {
        var service = new RepeatService();
        const ulong guildId = 10;

        service.Init(guildId);

        Assert.False(service.IsRepeating(guildId));
        Assert.False(service.IsRepeatingList(guildId));
    }

    [Fact]
    public void SetRepeating_AfterInit_UpdatesFlag()
    {
        var service = new RepeatService();
        const ulong guildId = 11;

        service.Init(guildId);
        service.SetRepeating(guildId, true);

        Assert.True(service.IsRepeating(guildId));
    }

    [Fact]
    public void SetRepeating_WithoutInit_DoesNothing()
    {
        var service = new RepeatService();
        const ulong guildId = 12;

        service.SetRepeating(guildId, true);

        Assert.False(service.IsRepeating(guildId));
    }

    [Fact]
    public void SetRepeatingList_AfterInit_UpdatesFlag()
    {
        var service = new RepeatService();
        const ulong guildId = 13;

        service.Init(guildId);
        service.SetRepeatingList(guildId, true);

        Assert.True(service.IsRepeatingList(guildId));
    }

    [Fact]
    public void SetRepeatingList_WithoutInit_DoesNothing()
    {
        var service = new RepeatService();
        const ulong guildId = 14;

        service.SetRepeatingList(guildId, true);

        Assert.False(service.IsRepeatingList(guildId));
    }
}