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
        new GameDetectionService(),
        new GameLaunchService())
    {
    }

    public MainViewModel(
        IConfigurationService configService,
        IProfileService profileService,
        IGameDetectionService detectionService,
        IGameLaunchService launchService)
    {
        _configService = configService;
        _profileService = profileService;
        _detectionService = detectionService;
        _launchService = launchService;

        // Navigation Commands
        SelectHomeCommand = new RelayCommand(() => ActiveTab = "Home");
        SelectLobbyCommand = new RelayCommand(() => ActiveTab = "Lobby");
        SelectSettingsCommand = new RelayCommand(() => ActiveTab = "Settings");
        SelectAboutCommand = new RelayCommand(() => ActiveTab = "About");

        // Action Commands
        PlayGameCommand = new RelayCommand(OnPlayGame, () => !IsBusy && Installation.IsReadyToPlay);
        SaveProfileCommand = new RelayCommand(async () => await SaveProfileAsync());
        RefreshGameStatusCommand = new RelayCommand(RefreshGameStatus);

        // Async initialization
        _ = InitializeAsync();
    }

    public string ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

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

    public string GameStatusSummary
    {
        get
        {
            if (!Installation.IsDetected)
                return "Original game directory not detected. Set path in Settings.";
            if (Installation.MissingFiles.Count > 0)
                return $"Missing essential files: {string.Join(", ", Installation.MissingFiles)}";
            return $"Game client verified ({Installation.ClientVersion})";
        }
    }

    public ICommand SelectHomeCommand { get; }
    public ICommand SelectLobbyCommand { get; }
    public ICommand SelectSettingsCommand { get; }
    public ICommand SelectAboutCommand { get; }
    public ICommand PlayGameCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand RefreshGameStatusCommand { get; }

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
    }

    private async Task SaveProfileAsync()
    {
        await _profileService.SaveProfileAsync(Profile);
        StatusMessage = $"Profile updated for '{Profile.Nickname}'";
    }

    private void OnPlayGame()
    {
        IsBusy = true;
        LaunchStatus = "Launching...";
        StatusMessage = "Starting Rules of Survival client...";

        bool success = _launchService.LaunchGame(Installation, Settings, out string error);
        if (success)
        {
            LaunchStatus = "Game Running";
            StatusMessage = "Rules of Survival process active";
            Profile.LastPlayedAt = DateTime.UtcNow;
            _ = _profileService.SaveProfileAsync(Profile);
        }
        else
        {
            LaunchStatus = "Launch Failed";
            StatusMessage = $"Error: {error}";
        }

        IsBusy = false;
    }
}
