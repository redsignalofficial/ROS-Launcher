using System.Windows.Input;
using ROS.Launcher.Models;
using ROS.Launcher.Services;

namespace ROS.Launcher.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IProfileService _profileService;
    private readonly IGameDetectionService _detectionService;
    private readonly IGameLaunchService _launchService;
    private readonly ILauncherLogService _logger;

    private string _activeTab = "Home";
    private GuestProfile _profile = new();
    private LauncherSettings _settings = new();
    private GameInstallation _installation = new();
    private string _statusMessage = "Offline Guest Mode Active";
    private bool _isBusy;
    private string _launchStatus = "Ready";

    public MainViewModel() : this(
        new JsonConfigurationService(),
        new LocalProfileService(),
        new GameDetectionService(new LauncherLogService()),
        new GameLaunchService(new LauncherLogService()),
        new LauncherLogService())
    {
    }

    public MainViewModel(
        IConfigurationService configService,
        IProfileService profileService,
        IGameDetectionService detectionService,
        IGameLaunchService launchService,
        ILauncherLogService logger)
    {
        _configService = configService;
        _profileService = profileService;
        _detectionService = detectionService;
        _launchService = launchService;
        _logger = logger;

        // Navigation Commands
        SelectHomeCommand = new RelayCommand(() => ActiveTab = "Home");
        SelectLobbyCommand = new RelayCommand(() => ActiveTab = "Lobby");
        SelectSettingsCommand = new RelayCommand(() => ActiveTab = "Settings");
        SelectAboutCommand = new RelayCommand(() => ActiveTab = "About");

        // Action Commands
        PlayGameCommand = new RelayCommand(OnPlayGame, () => !IsBusy && Installation.IsReadyToPlay);
        SaveProfileCommand = new RelayCommand(async () => await SaveProfileAsync());
        SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
        RefreshGameStatusCommand = new RelayCommand(RefreshGameStatus);
        BrowseGameDirectoryCommand = new RelayCommand(OnBrowseGameDirectory);

        // Listen for game exit
        _launchService.GameExited += OnGameExited;

        _logger.LogInfo("ROS Revival Launcher initialized.");

        // Async initialization
        _ = InitializeAsync();
    }

    public string ActiveTab
    {
        get => _activeTab;
        set
        {
            if (SetProperty(ref _activeTab, value))
            {
                OnPropertyChanged(nameof(IsHomeActive));
                OnPropertyChanged(nameof(IsLobbyActive));
                OnPropertyChanged(nameof(IsSettingsActive));
                OnPropertyChanged(nameof(IsAboutActive));
            }
        }
    }

    public bool IsHomeActive => ActiveTab == "Home";
    public bool IsLobbyActive => ActiveTab == "Lobby";
    public bool IsSettingsActive => ActiveTab == "Settings";
    public bool IsAboutActive => ActiveTab == "About";

    public GuestProfile Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public LauncherSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public GameInstallation Installation
    {
        get => _installation;
        set
        {
            if (SetProperty(ref _installation, value))
            {
                OnPropertyChanged(nameof(GameStatusSummary));
                OnPropertyChanged(nameof(CanPlay));
                OnPropertyChanged(nameof(LastIntegrityCheckText));
                OnPropertyChanged(nameof(MissingFilesText));
                OnPropertyChanged(nameof(HasMissingFiles));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanPlay));
            }
        }
    }

    public string LaunchStatus
    {
        get => _launchStatus;
        set => SetProperty(ref _launchStatus, value);
    }

    public bool CanPlay => !IsBusy && Installation.IsReadyToPlay;

    public string GameStatusSummary => Installation.StatusDescription;

    public string LastIntegrityCheckText =>
        $"Checked at {Installation.LastChecked.ToLocalTime():HH:mm:ss}";

    public bool HasMissingFiles => Installation.MissingFiles.Count > 0;

    public string MissingFilesText =>
        HasMissingFiles ? string.Join(", ", Installation.MissingFiles) : "All core files verified";

    public ICommand SelectHomeCommand { get; }
    public ICommand SelectLobbyCommand { get; }
    public ICommand SelectSettingsCommand { get; }
    public ICommand SelectAboutCommand { get; }
    public ICommand PlayGameCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand RefreshGameStatusCommand { get; }
    public ICommand BrowseGameDirectoryCommand { get; }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Settings = await _configService.LoadSettingsAsync();
            Profile = await _profileService.LoadOrCreateProfileAsync();
            RefreshGameStatus();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void RefreshGameStatus()
    {
        Installation = _detectionService.DetectInstallation(Settings.GameDirectory);
        _logger.LogInfo($"Game status refreshed. Path: '{Installation.GameDirectory}', Ready: {Installation.IsReadyToPlay}");
    }

    public async Task SaveSettingsAsync()
    {
        await _configService.SaveSettingsAsync(Settings);
        RefreshGameStatus();
        StatusMessage = "Settings saved successfully.";
        _logger.LogInfo($"Settings saved. Configured game path: '{Settings.GameDirectory}'");
    }

    private async Task SaveProfileAsync()
    {
        await _profileService.SaveProfileAsync(Profile);
        StatusMessage = $"Profile updated for '{Profile.Nickname}'";
        _logger.LogInfo($"Profile updated: Nickname '{Profile.Nickname}', ID '{Profile.LocalId}'");
    }

    private void OnBrowseGameDirectory()
    {
        try
        {
            // OpenFolderDialog in modern WPF / Windows Forms
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Original Rules of Survival Game Directory",
                InitialDirectory = string.IsNullOrWhiteSpace(Settings.GameDirectory) ? "" : Settings.GameDirectory
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
            {
                Settings.GameDirectory = dialog.FolderName;
                OnPropertyChanged(nameof(Settings));
                _ = SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error opening folder browser dialog", ex);
            StatusMessage = "Please manually enter the game directory path in Settings.";
        }
    }

    private void OnPlayGame()
    {
        IsBusy = true;
        LaunchStatus = "Starting ros.exe...";
        StatusMessage = "Launching original Rules of Survival client...";

        bool success = _launchService.LaunchGame(Installation, Settings, out string error);
        if (success)
        {
            LaunchStatus = "Game Active";
            StatusMessage = "Game process is running. Note: Official servers are offline.";
            Profile.LastPlayedAt = DateTime.UtcNow;
            _ = _profileService.SaveProfileAsync(Profile);
        }
        else
        {
            LaunchStatus = "Ready";
            StatusMessage = error;
            _logger.LogWarning($"Play command failed: {error}");
        }

        IsBusy = false;
    }

    private void OnGameExited(object? sender, int exitCode)
    {
        LaunchStatus = "Ready";
        StatusMessage = $"Game process closed (Exit code: {exitCode}).";
        OnPropertyChanged(nameof(CanPlay));
    }
}
