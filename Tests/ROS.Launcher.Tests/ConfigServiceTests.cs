using System.IO;
using ROS.Launcher.Models;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class ConfigServiceTests : IDisposable
{
    private readonly string _testDir;

    public ConfigServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Config_" + Guid.NewGuid().ToString("N"));
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
    public async Task LoadSettingsAsync_WhenNoFileExists_CreatesDefaultSettings()
    {
        var service = new JsonConfigurationService(_testDir);

        var settings = await service.LoadSettingsAsync();

        Assert.NotNull(settings);
        Assert.Equal("1920x1080", settings.SelectedResolution);
        Assert.False(settings.AutoCloseOnLaunch);
        Assert.True(File.Exists(service.GetSettingsFilePath()));
    }

    [Fact]
    public async Task SaveSettingsAsync_PersistsCustomValuesCorrectly()
    {
        var service = new JsonConfigurationService(_testDir);
        var customSettings = new LauncherSettings
        {
            GameDirectory = @"C:\Games\RulesOfSurvival",
            AutoCloseOnLaunch = true,
            LaunchArguments = "-windowed -fps 60",
            SelectedResolution = "2560x1440"
        };

        await service.SaveSettingsAsync(customSettings);
        var loaded = await service.LoadSettingsAsync();

        Assert.NotNull(loaded);
        Assert.Equal(customSettings.GameDirectory, loaded.GameDirectory);
        Assert.True(loaded.AutoCloseOnLaunch);
        Assert.Equal("-windowed -fps 60", loaded.LaunchArguments);
        Assert.Equal("2560x1440", loaded.SelectedResolution);
    }
}
