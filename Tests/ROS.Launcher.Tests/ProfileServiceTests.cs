using System.IO;
using ROS.Launcher.Models;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class ProfileServiceTests : IDisposable
{
    private readonly string _testDir;

    public ProfileServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Profile_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }
        catch { }
    }

    [Fact]
    public async Task LoadOrCreateProfileAsync_WhenNoFileExists_GeneratesGuestSurvivorProfile()
    {
        var service = new LocalProfileService(_testDir);

        var profile = await service.LoadOrCreateProfileAsync();

        Assert.NotNull(profile);
        Assert.StartsWith("Survivor_", profile.Nickname);
        Assert.False(string.IsNullOrWhiteSpace(profile.LocalId));
        Assert.Equal(1, profile.LocalLevel);
    }

    [Fact]
    public void GenerateDefaultGuestNickname_ReturnsSurvivorFormat()
    {
        var service = new LocalProfileService(_testDir);

        string nickname = service.GenerateDefaultGuestNickname();

        Assert.StartsWith("Survivor_", nickname);
        Assert.True(nickname.Length >= 13);
    }

    [Fact]
    public async Task SaveProfileAsync_PersistsProfileChanges()
    {
        var service = new LocalProfileService(_testDir);
        var initial = await service.LoadOrCreateProfileAsync();

        initial.Nickname = "Vanguard_Test";
        initial.LocalLevel = 5;
        await service.SaveProfileAsync(initial);

        var reloaded = await service.LoadOrCreateProfileAsync();

        Assert.Equal("Vanguard_Test", reloaded.Nickname);
        Assert.Equal(5, reloaded.LocalLevel);
    }
}
