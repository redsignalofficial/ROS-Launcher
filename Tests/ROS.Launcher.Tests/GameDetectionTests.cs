using System.IO;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class GameDetectionTests : IDisposable
{
    private readonly string _testDir;

    public GameDetectionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Detection_" + Guid.NewGuid().ToString("N"));
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
    public void DetectInstallation_WhenEmptyPath_ReturnsNotDetected()
    {
        var service = new GameDetectionService();

        var result = service.DetectInstallation(Path.Combine(_testDir, "non_existent"));

        Assert.False(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
    }

    [Fact]
    public void DetectInstallation_WhenValidFilesExist_ReturnsReadyToPlay()
    {
        var service = new GameDetectionService();
        string mockGameDir = Path.Combine(_testDir, "Game");
        Directory.CreateDirectory(mockGameDir);
        File.WriteAllText(Path.Combine(mockGameDir, "ros.exe"), "mock_exe");
        File.WriteAllText(Path.Combine(mockGameDir, "assets.npk"), "mock_assets");
        File.WriteAllText(Path.Combine(mockGameDir, "script.npk"), "mock_script");
        File.WriteAllText(Path.Combine(mockGameDir, "version.dat"), "1.9.0-test");

        var result = service.DetectInstallation(mockGameDir);

        Assert.True(result.IsDetected);
        Assert.True(result.IsReadyToPlay);
        Assert.Empty(result.MissingFiles);
        Assert.Equal("1.9.0-test", result.ClientVersion);
    }

    [Fact]
    public void DetectInstallation_WhenMissingNpk_ReportsMissingFile()
    {
        var service = new GameDetectionService();
        string mockGameDir = Path.Combine(_testDir, "Game");
        Directory.CreateDirectory(mockGameDir);
        File.WriteAllText(Path.Combine(mockGameDir, "ros.exe"), "mock_exe");
        // missing assets.npk and script.npk

        var result = service.DetectInstallation(mockGameDir);

        Assert.True(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
        Assert.Contains("assets.npk", result.MissingFiles);
        Assert.Contains("script.npk", result.MissingFiles);
    }
}
