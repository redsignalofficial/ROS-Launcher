using System.Diagnostics;
using System.IO;
using ROS.Launcher.Models;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class GameLaunchTests : IDisposable
{
    private readonly string _testDir;
    private readonly LauncherLogService _logger;

    public GameLaunchTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Launch_" + Guid.NewGuid().ToString("N"));
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
    public void LaunchGame_WhenClientNotReady_ReturnsFalseAndError()
    {
        var launchService = new GameLaunchService(_logger);
        var unreadyInstallation = new GameInstallation
        {
            IsDetected = true,
            GameDirectory = _testDir,
            ExecutablePath = Path.Combine(_testDir, "ros.exe"),
            MissingFiles = new List<string> { "assets.npk", "script.npk" }
        };
        var settings = new LauncherSettings();

        bool success = launchService.LaunchGame(unreadyInstallation, settings, out string error);

        Assert.False(success);
        Assert.Contains("Missing files", error);
        Assert.Contains("assets.npk", error);

        // Verify logged in launcher log
        string logContent = File.ReadAllText(_logger.GetLogFilePath());
        Assert.Contains("[WARN]", logContent);
        Assert.Contains("Launch rejected", logContent);
    }

    [Fact]
    public void LaunchGame_WhenExecutableDoesNotExist_ReturnsFalse()
    {
        var launchService = new GameLaunchService(_logger);
        var installation = new GameInstallation
        {
            IsDetected = true,
            GameDirectory = _testDir,
            ExecutablePath = Path.Combine(_testDir, "non_existent_ros.exe"),
            MissingFiles = new List<string>() // empty, but file does not exist on disk
        };
        var settings = new LauncherSettings();

        bool success = launchService.LaunchGame(installation, settings, out string error);

        Assert.False(success);
        Assert.Contains("not found", error);

        string logContent = File.ReadAllText(_logger.GetLogFilePath());
        Assert.Contains("[ERROR]", logContent);
        Assert.Contains("Executable not found", logContent);
    }

    [Fact]
    public void LaunchGame_PreventsDuplicateLaunch_WhenAlreadyActive()
    {
        var launchService = new TestableGameLaunchService(_logger, isAlreadyRunning: true, mockRunningPid: 8844);
        var installation = new GameInstallation
        {
            IsDetected = true,
            GameDirectory = _testDir,
            ExecutablePath = Path.Combine(_testDir, "ros.exe"),
            MissingFiles = new List<string>()
        };
        File.WriteAllText(installation.ExecutablePath, "dummy");
        var settings = new LauncherSettings();

        bool success = launchService.LaunchGame(installation, settings, out string error);

        Assert.False(success);
        Assert.Contains("already running", error);
        Assert.Contains("8844", error);

        string logContent = File.ReadAllText(_logger.GetLogFilePath());
        Assert.Contains("[WARN]", logContent);
        Assert.Contains("Duplicate launch attempt blocked", logContent);
    }

    // Helper subclass for testing duplicate launch state deterministically
    private class TestableGameLaunchService : GameLaunchService
    {
        private readonly bool _mockRunning;
        private readonly int _mockPid;

        public TestableGameLaunchService(ILauncherLogService logger, bool isAlreadyRunning, int mockRunningPid)
            : base(logger)
        {
            _mockRunning = isAlreadyRunning;
            _mockPid = mockRunningPid;
        }

        public override bool IsGameRunning(out int processId)
        {
            if (_mockRunning)
            {
                processId = _mockPid;
                return true;
            }
            return base.IsGameRunning(out processId);
        }
    }
}
