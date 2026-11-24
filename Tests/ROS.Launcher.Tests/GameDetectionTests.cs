using System.IO;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class GameDetectionTests : IDisposable
{
    private readonly string _testDir;
    private readonly LauncherLogService _logger;

    public GameDetectionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Detection_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        _logger = new LauncherLogService(_testDir);
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
    public void DetectInstallation_WhenDirectoryDoesNotExist_ReturnsNotDetected()
    {
        var service = new GameDetectionService(_logger);
        string nonExistentPath = Path.Combine(_testDir, "NonExistentFolder_" + Guid.NewGuid().ToString("N"));

        var result = service.DetectInstallation(nonExistentPath);

        Assert.False(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
        Assert.Contains("not detected", result.StatusDescription);
    }

    [Fact]
    public void DetectInstallation_WhenMissingRosExe_ReturnsNotDetected()
    {
        var service = new GameDetectionService(_logger);
        string folderWithoutExe = Path.Combine(_testDir, "GameFolderNoExe");
        Directory.CreateDirectory(folderWithoutExe);
        File.WriteAllText(Path.Combine(folderWithoutExe, "assets.npk"), "dummy");
        File.WriteAllText(Path.Combine(folderWithoutExe, "script.npk"), "dummy");

        var result = service.DetectInstallation(folderWithoutExe);

        Assert.False(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
    }

    [Fact]
    public void DetectInstallation_WhenValidFilesExist_ReturnsReadyToPlay()
    {
        var service = new GameDetectionService(_logger);
        string validGameDir = Path.Combine(_testDir, "Game");
        Directory.CreateDirectory(validGameDir);
        File.WriteAllText(Path.Combine(validGameDir, "ros.exe"), "dummy_pe");
        File.WriteAllText(Path.Combine(validGameDir, "assets.npk"), "dummy_assets");
        File.WriteAllText(Path.Combine(validGameDir, "script.npk"), "dummy_script");
        File.WriteAllText(Path.Combine(validGameDir, "setting.ini"), "dummy_ini");
        File.WriteAllText(Path.Combine(validGameDir, "version.dat"), "1.9.0-test");

        var result = service.DetectInstallation(validGameDir);

        Assert.True(result.IsDetected);
        Assert.True(result.IsReadyToPlay);
        Assert.Empty(result.MissingFiles);
        Assert.Contains("ros.exe", result.ValidatedFiles);
        Assert.Contains("assets.npk", result.ValidatedFiles);
        Assert.Contains("script.npk", result.ValidatedFiles);
        Assert.Contains("setting.ini", result.ValidatedFiles);
        Assert.Equal("1.9.0-test", result.ClientVersion);
    }

    [Fact]
    public void DetectInstallation_WhenMissingExpectedGameFiles_ReportsMissingList()
    {
        var service = new GameDetectionService(_logger);
        string incompleteGameDir = Path.Combine(_testDir, "IncompleteGame");
        Directory.CreateDirectory(incompleteGameDir);
        File.WriteAllText(Path.Combine(incompleteGameDir, "ros.exe"), "dummy_pe");
        File.WriteAllText(Path.Combine(incompleteGameDir, "setting.ini"), "dummy_ini");
        // assets.npk and script.npk are deliberately omitted

        var result = service.DetectInstallation(incompleteGameDir);

        Assert.True(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
        Assert.Contains("assets.npk", result.MissingFiles);
        Assert.Contains("script.npk", result.MissingFiles);
        Assert.Contains("ros.exe", result.ValidatedFiles);
        Assert.Contains("Missing essential files", result.StatusDescription);
    }

    [Fact]
    public void DetectInstallation_WhenParentDirectorySelected_FindsSubfolderGame()
    {
        var service = new GameDetectionService(_logger);
        string rootDir = Path.Combine(_testDir, "RulesOfSurvivalRoot");
        string gameSubdir = Path.Combine(rootDir, "Game");
        Directory.CreateDirectory(gameSubdir);
        File.WriteAllText(Path.Combine(gameSubdir, "ros.exe"), "dummy_pe");
        File.WriteAllText(Path.Combine(gameSubdir, "assets.npk"), "dummy_assets");
        File.WriteAllText(Path.Combine(gameSubdir, "script.npk"), "dummy_script");
        File.WriteAllText(Path.Combine(gameSubdir, "setting.ini"), "dummy_ini");

        // User points to root folder rather than Game subfolder
        var result = service.DetectInstallation(rootDir);

        Assert.True(result.IsDetected);
        Assert.True(result.IsReadyToPlay);
        Assert.Equal(Path.GetFullPath(gameSubdir), result.GameDirectory);
    }
}
