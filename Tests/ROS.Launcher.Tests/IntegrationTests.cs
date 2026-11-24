using System.Diagnostics;
using System.IO;
using ROS.Launcher.Models;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _testRoot;
    private readonly string _configDir;
    private readonly string _mockGameDir;

    public IntegrationTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "ROSRevival_Integration_" + Guid.NewGuid().ToString("N"));
        _configDir = Path.Combine(_testRoot, "Config");
        _mockGameDir = Path.Combine(_testRoot, "MockGame");

        Directory.CreateDirectory(_configDir);
        Directory.CreateDirectory(_mockGameDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, true);
        }
        catch { }
    }

    [Fact]
    public async Task Test_GuestMode_FullPersistenceCycle()
    {
        // 1. Initialize Profile Service with dedicated test directory
        var profileService = new LocalProfileService(_configDir);

        // 2. Generate and verify initial guest profile
        var profile = await profileService.LoadOrCreateProfileAsync();
        Assert.NotNull(profile);
        Assert.StartsWith("Survivor_", profile.Nickname);
        Assert.False(string.IsNullOrWhiteSpace(profile.LocalId));

        // 3. Edit guest nickname and local level
        profile.Nickname = "EliteSurvivor_Test";
        profile.LocalLevel = 10;
        await profileService.SaveProfileAsync(profile);

        // 4. Reload from fresh service instance to prove persistence
        var reloadedService = new LocalProfileService(_configDir);
        var reloadedProfile = await reloadedService.LoadOrCreateProfileAsync();
        Assert.Equal("EliteSurvivor_Test", reloadedProfile.Nickname);
        Assert.Equal(10, reloadedProfile.LocalLevel);
        Assert.Equal(profile.LocalId, reloadedProfile.LocalId);
    }

    [Fact]
    public async Task Test_Settings_PersistenceAndModification()
    {
        var configService = new JsonConfigurationService(_configDir);

        var settings = await configService.LoadSettingsAsync();
        Assert.NotNull(settings);

        settings.GameDirectory = _mockGameDir;
        settings.AutoCloseOnLaunch = true;
        settings.LaunchArguments = "-windowed -noborder";
        settings.SelectedResolution = "2560x1440";
        settings.IsMuted = true;
        await configService.SaveSettingsAsync(settings);

        var reloadedService = new JsonConfigurationService(_configDir);
        var reloadedSettings = await reloadedService.LoadSettingsAsync();
        Assert.Equal(_mockGameDir, reloadedSettings.GameDirectory);
        Assert.True(reloadedSettings.AutoCloseOnLaunch);
        Assert.Equal("-windowed -noborder", reloadedSettings.LaunchArguments);
        Assert.Equal("2560x1440", reloadedSettings.SelectedResolution);
        Assert.True(reloadedSettings.IsMuted);
    }

    [Fact]
    public void Test_ControlledDirectory_FullDetectionLifecycle()
    {
        var detectionService = new GameDetectionService();

        // Populate complete mock game directory
        File.WriteAllText(Path.Combine(_mockGameDir, "ros.exe"), "mock_pe_executable");
        File.WriteAllText(Path.Combine(_mockGameDir, "assets.npk"), "mock_assets_npk");
        File.WriteAllText(Path.Combine(_mockGameDir, "script.npk"), "mock_script_npk");
        File.WriteAllText(Path.Combine(_mockGameDir, "setting.ini"), "mock_ini");
        File.WriteAllText(Path.Combine(_mockGameDir, "version.dat"), "1.9.0-controlled-test");

        var result = detectionService.DetectInstallation(_mockGameDir);

        Assert.True(result.IsDetected);
        Assert.True(result.IsReadyToPlay);
        Assert.Empty(result.MissingFiles);
        Assert.Equal("1.9.0-controlled-test", result.ClientVersion);
    }

    [Fact]
    public void Test_ControlledDirectory_MissingFileHandling()
    {
        var detectionService = new GameDetectionService();

        // Create partial mock game directory (missing script.npk)
        File.WriteAllText(Path.Combine(_mockGameDir, "ros.exe"), "mock_pe_executable");
        File.WriteAllText(Path.Combine(_mockGameDir, "assets.npk"), "mock_assets_npk");

        var result = detectionService.DetectInstallation(_mockGameDir);

        Assert.True(result.IsDetected);
        Assert.False(result.IsReadyToPlay);
        Assert.Contains("script.npk", result.MissingFiles);
    }

    [Fact]
    public void Test_LaunchService_WhenGameNotReady_FailsSafely()
    {
        var launchService = new GameLaunchService();
        var unreadyInstallation = new GameInstallation
        {
            IsDetected = true,
            ExecutablePath = Path.Combine(_mockGameDir, "ros.exe"),
            MissingFiles = new List<string> { "script.npk" }
        };
        var settings = new LauncherSettings();

        bool launched = launchService.LaunchGame(unreadyInstallation, settings, out string error);

        Assert.False(launched);
        Assert.Contains("Missing files", error);
    }

    [Fact]
    public async Task Test_CustomLobby_FullLifecycle_And_StartMatchValidation()
    {
        var logger = new LauncherLogService(_configDir);
        var lobbyService = new LocalLobbyService(logger, _configDir);
        var launchService = new GameLaunchService(logger);

        // 1. Initialize Custom Lobby
        var lobby = await lobbyService.LoadOrCreateLobbyAsync("CompetitivePlayer");
        Assert.NotNull(lobby);
        Assert.Equal(6, lobby.RoomId.Length);
        Assert.Equal("CompetitivePlayer", lobby.Slots[0].PlayerName);

        // 2. Configure match parameters
        lobby.SelectedMap = "Fearless Fiord";
        var modeResult = lobbyService.ChangeMode(lobby, "Squad");
        Assert.Equal(CapacityChangeStatus.Success, modeResult.Status);

        // 3. Add visual placeholder bot
        bool botAdded = lobbyService.AddBotSlot(lobby, out string botMsg);
        Assert.True(botAdded);
        Assert.True(lobby.Slots[1].IsBot);

        // 4. Save lobby configuration
        await lobbyService.SaveLobbyAsync(lobby);
        Assert.True(File.Exists(lobbyService.GetLobbyFilePath()));

        // 5. Verify Start Match prerequisites: game not ready must block start
        var unreadyInstall = new GameInstallation
        {
            IsDetected = false,
            ExecutablePath = ""
        };
        var settings = new LauncherSettings();

        bool matchStarted = launchService.LaunchGame(unreadyInstall, settings, out string launchError);
        Assert.False(matchStarted);
        Assert.Contains("Game client is not ready", launchError);

        // 6. Test capacity reduction warning protection
        var warnResult = lobbyService.ChangeMode(lobby, "Solo", confirmTruncate: false);
        Assert.Equal(CapacityChangeStatus.WarningOccupiedSlotsWouldBeRemoved, warnResult.Status);
        Assert.Equal(4, lobby.Slots.Count); // Preserved!
    }
}
