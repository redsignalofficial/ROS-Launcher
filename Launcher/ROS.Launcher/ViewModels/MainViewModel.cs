using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly ILobbyService _lobbyService;
    private readonly ILauncherLogService _logger;

    private string _activeTab = "Home";
    private GuestProfile _profile = new();
    private LauncherSettings _settings = new();
    private GameInstallation _installation = new();
    private LobbyRoom _lobby = new();
    private string _statusMessage = "Offline Guest Mode Active";
    private bool _isBusy;
    private string _launchStatus = "Ready";
    private string _capacityWarningMessage = string.Empty;
    private bool _hasCapacityWarning;
    private string _pendingMode = string.Empty;

    public MainViewModel() : this(
        new JsonConfigurationService(),
        new LocalProfileService(),
        new GameDetectionService(new LauncherLogService()),
        new GameLaunchService(new LauncherLogService()),
        new LocalLobbyService(new LauncherLogService()),
        new LauncherLogService())
    {
    }

    public MainViewModel(
        IConfigurationService configService,
        IProfileService profileService,
        IGameDetectionService detectionService,
        IGameLaunchService launchService,
        ILauncherLogService logger) : this(
        configService,
        profileService,
        detectionService,
        launchService,
        new LocalLobbyService(logger),
        logger)
    {
    }

    public MainViewModel(
        IConfigurationService configService,
        IProfileService profileService,
        IGameDetectionService detectionService,
        IGameLaunchService launchService,
        ILobbyService lobbyService,
        ILauncherLogService logger)
    {
        _configService = configService;
        _profileService = profileService;
        _detectionService = detectionService;
        _launchService = launchService;
        _lobbyService = lobbyService;
        _logger = logger;

        // Navigation Commands
        SelectHomeCommand = new RelayCommand(() => ActiveTab = "Home");
        SelectLobbyCommand = new RelayCommand(() => ActiveTab = "Lobby");
        SelectSettingsCommand = new RelayCommand(() => ActiveTab = "Settings");
        SelectAboutCommand = new RelayCommand(() => ActiveTab = "About");

        // Action Commands - Game
        PlayGameCommand = new RelayCommand(OnPlayGame, () => !IsBusy && Installation.IsReadyToPlay);
        SaveProfileCommand = new RelayCommand(async () => await SaveProfileAsync());
        SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
        RefreshGameStatusCommand = new RelayCommand(RefreshGameStatus);
        BrowseGameDirectoryCommand = new RelayCommand(OnBrowseGameDirectory);

        // Action Commands - Custom Lobby
        CreateLobbyCommand = new RelayCommand(OnCreateLobby);
        SaveLobbyCommand = new RelayCommand(async () => await SaveLobbyAsync());
        SelectMapCommand = new RelayCommand(p => OnSelectMap(p?.ToString()));
        SelectModeCommand = new RelayCommand(p => OnSelectMode(p?.ToString()));
        ConfirmChangeModeCommand = new RelayCommand(OnConfirmChangeMode);
        CancelChangeModeCommand = new RelayCommand(OnCancelChangeMode);
        SelectPerspectiveCommand = new RelayCommand(p => OnSelectPerspective(p?.ToString()));
        AddBotCommand = new RelayCommand(OnAddBot, () => CanAddBot);
        ClearSlotCommand = new RelayCommand(OnClearSlot);
        StartMatchCommand = new RelayCommand(OnStartMatch, () => CanPlay);

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
                OnPropertyChanged(nameof(CanStartMatch));
                OnPropertyChanged(nameof(LastIntegrityCheckText));
                OnPropertyChanged(nameof(MissingFilesText));
                OnPropertyChanged(nameof(HasMissingFiles));
            }
        }
    }

    public LobbyRoom Lobby
    {
        get => _lobby;
        set
        {
            if (SetProperty(ref _lobby, value))
            {
                NotifyLobbyChanged();
            }
        }
    }

    public void NotifyLobbyChanged()
    {
        OnPropertyChanged(nameof(Lobby));
        OnPropertyChanged(nameof(Slots));
        OnPropertyChanged(nameof(CanAddBot));
        OnPropertyChanged(nameof(LobbyRoomIdText));
        OnPropertyChanged(nameof(LobbyStatusSummary));
        OnPropertyChanged(nameof(IsMapGhillie));
        OnPropertyChanged(nameof(IsMapFearless));
        OnPropertyChanged(nameof(IsModeSolo));
        OnPropertyChanged(nameof(IsModeDuo));
        OnPropertyChanged(nameof(IsModeSquad));
        OnPropertyChanged(nameof(IsModeFireteam));
        OnPropertyChanged(nameof(IsPerspectiveTpp));
        OnPropertyChanged(nameof(IsPerspectiveFpp));
    }

    public IReadOnlyList<LobbySlot> Slots => Lobby.Slots;
    public bool CanAddBot => Lobby.Slots.Any(s => !s.IsOccupied);
    public string LobbyRoomIdText => $"ROOM #{Lobby.RoomId}";
    public string LobbyStatusSummary => $"Map: {Lobby.SelectedMap}  •  Mode: {Lobby.SelectedMode} ({Lobby.MaxPlayers}P)  •  Perspective: {Lobby.Perspective}  •  Slots: {Lobby.OccupiedCount}/{Lobby.MaxPlayers}";

    public bool IsMapGhillie => Lobby.SelectedMap == "Ghillie Island";
    public bool IsMapFearless => Lobby.SelectedMap == "Fearless Fiord";

    public bool IsModeSolo => Lobby.SelectedMode == "Solo";
    public bool IsModeDuo => Lobby.SelectedMode == "Duo";
    public bool IsModeSquad => Lobby.SelectedMode == "Squad";
    public bool IsModeFireteam => Lobby.SelectedMode == "Fireteam";

    public bool IsPerspectiveTpp => Lobby.Perspective == "TPP";
    public bool IsPerspectiveFpp => Lobby.Perspective == "FPP";

    public string CapacityWarningMessage
    {
        get => _capacityWarningMessage;
        set => SetProperty(ref _capacityWarningMessage, value);
    }

    public bool HasCapacityWarning
    {
        get => _hasCapacityWarning;
        set => SetProperty(ref _hasCapacityWarning, value);
    }

    public string PendingMode
    {
        get => _pendingMode;
        set => SetProperty(ref _pendingMode, value);
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
                OnPropertyChanged(nameof(CanStartMatch));
            }
        }
    }

    public string LaunchStatus
    {
        get => _launchStatus;
        set => SetProperty(ref _launchStatus, value);
    }

    public bool CanPlay => !IsBusy && Installation.IsReadyToPlay;
    public bool CanStartMatch => !IsBusy && Installation.IsReadyToPlay;

    public string GameStatusSummary => Installation.StatusDescription;

    public string LastIntegrityCheckText =>
        $"Checked at {Installation.LastChecked.ToLocalTime():HH:mm:ss}";

    public bool HasMissingFiles => Installation.MissingFiles.Count > 0;

    public string MissingFilesText =>
        HasMissingFiles ? string.Join(", ", Installation.MissingFiles) : "All core files verified";

    // Navigation Commands
    public ICommand SelectHomeCommand { get; }
    public ICommand SelectLobbyCommand { get; }
    public ICommand SelectSettingsCommand { get; }
    public ICommand SelectAboutCommand { get; }

    // Action Commands - Game
    public ICommand PlayGameCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand RefreshGameStatusCommand { get; }
    public ICommand BrowseGameDirectoryCommand { get; }

    // Action Commands - Custom Lobby
    public ICommand CreateLobbyCommand { get; }
    public ICommand SaveLobbyCommand { get; }
    public ICommand SelectMapCommand { get; }
    public ICommand SelectModeCommand { get; }
    public ICommand ConfirmChangeModeCommand { get; }
    public ICommand CancelChangeModeCommand { get; }
    public ICommand SelectPerspectiveCommand { get; }
    public ICommand AddBotCommand { get; }
    public ICommand ClearSlotCommand { get; }
    public ICommand StartMatchCommand { get; }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Settings = await _configService.LoadSettingsAsync();
            Profile = await _profileService.LoadOrCreateProfileAsync();
            Lobby = await _lobbyService.LoadOrCreateLobbyAsync(Profile.Nickname);
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

    public async Task SaveProfileAsync()
    {
        await _profileService.SaveProfileAsync(Profile);
        _lobbyService.SyncHostName(Lobby, Profile.Nickname);
        await _lobbyService.SaveLobbyAsync(Lobby);
        NotifyLobbyChanged();
        StatusMessage = $"Profile updated for '{Profile.Nickname}'";
        _logger.LogInfo($"Profile updated: Nickname '{Profile.Nickname}', ID '{Profile.LocalId}'");
    }

    public async Task SaveLobbyAsync()
    {
        await _lobbyService.SaveLobbyAsync(Lobby);
        StatusMessage = "Lobby configuration saved locally.";
        _logger.LogInfo($"Lobby configuration saved. Room ID: #{Lobby.RoomId}");
    }

    private void OnCreateLobby()
    {
        Lobby = _lobbyService.CreateDefaultLobby(Profile.Nickname);
        HasCapacityWarning = false;
        CapacityWarningMessage = string.Empty;
        PendingMode = string.Empty;
        _ = _lobbyService.SaveLobbyAsync(Lobby);
        StatusMessage = $"Created new local custom room #{Lobby.RoomId}.";
        _logger.LogInfo($"New local custom room #{Lobby.RoomId} created by {Profile.Nickname}.");
    }

    private void OnSelectMap(string? map)
    {
        if (string.IsNullOrWhiteSpace(map)) return;
        Lobby.SelectedMap = map;
        NotifyLobbyChanged();
        _ = _lobbyService.SaveLobbyAsync(Lobby);
        StatusMessage = $"Selected map: {map}";
        _logger.LogInfo($"Custom Lobby map changed to '{map}'.");
    }

    private void OnSelectMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return;
        var result = _lobbyService.ChangeMode(Lobby, mode, confirmTruncate: false);
        if (result.Status == CapacityChangeStatus.WarningOccupiedSlotsWouldBeRemoved)
        {
            PendingMode = mode;
            CapacityWarningMessage = result.Message;
            HasCapacityWarning = true;
            StatusMessage = result.Message;
            _logger.LogWarning($"Capacity reduction warning: {result.Message}");
        }
        else if (result.Status == CapacityChangeStatus.Success)
        {
            HasCapacityWarning = false;
            CapacityWarningMessage = string.Empty;
            PendingMode = string.Empty;
            NotifyLobbyChanged();
            _ = _lobbyService.SaveLobbyAsync(Lobby);
            StatusMessage = result.Message;
            _logger.LogInfo(result.Message);
        }
    }

    private void OnConfirmChangeMode()
    {
        if (!string.IsNullOrEmpty(PendingMode))
        {
            var result = _lobbyService.ChangeMode(Lobby, PendingMode, confirmTruncate: true);
            string oldMode = PendingMode;
            PendingMode = string.Empty;
            HasCapacityWarning = false;
            CapacityWarningMessage = string.Empty;
            NotifyLobbyChanged();
            _ = _lobbyService.SaveLobbyAsync(Lobby);
            StatusMessage = $"Confirmed mode change to {oldMode}. Excess slots removed.";
            _logger.LogInfo($"Confirmed capacity reduction to {oldMode}. Slots truncated.");
        }
    }

    private void OnCancelChangeMode()
    {
        HasCapacityWarning = false;
        CapacityWarningMessage = string.Empty;
        PendingMode = string.Empty;
        StatusMessage = "Mode change cancelled. Occupied slots preserved.";
    }

    private void OnSelectPerspective(string? persp)
    {
        if (string.IsNullOrWhiteSpace(persp)) return;
        Lobby.Perspective = persp;
        NotifyLobbyChanged();
        _ = _lobbyService.SaveLobbyAsync(Lobby);
        StatusMessage = $"Perspective set to {persp}";
        _logger.LogInfo($"Custom Lobby perspective set to '{persp}'.");
    }

    private void OnAddBot()
    {
        if (_lobbyService.AddBotSlot(Lobby, out string message))
        {
            NotifyLobbyChanged();
            _ = _lobbyService.SaveLobbyAsync(Lobby);
            StatusMessage = message;
            _logger.LogInfo(message);
        }
        else
        {
            StatusMessage = message;
            _logger.LogWarning(message);
        }
    }

    private void OnClearSlot(object? param)
    {
        int slotNumber = 0;
        if (param is int n) slotNumber = n;
        else if (param is string s && int.TryParse(s, out int parsed)) slotNumber = parsed;
        else if (param is LobbySlot slot) slotNumber = slot.SlotNumber;

        if (slotNumber > 0)
        {
            if (_lobbyService.ClearSlot(Lobby, slotNumber, out string message))
            {
                NotifyLobbyChanged();
                _ = _lobbyService.SaveLobbyAsync(Lobby);
                StatusMessage = message;
                _logger.LogInfo(message);
            }
            else
            {
                StatusMessage = message;
                _logger.LogWarning(message);
            }
        }
    }

    private void OnStartMatch()
    {
        if (!Installation.IsReadyToPlay)
        {
            StatusMessage = "Cannot start match: ROS game directory not configured or files are missing.";
            LaunchStatus = "Game Not Ready";
            return;
        }

        IsBusy = true;
        LaunchStatus = "Starting ros.exe...";
        StatusMessage = $"Launching original ROS client for Room #{Lobby.RoomId}... Note: Official servers are offline.";

        _logger.LogInfo($"Start Match triggered from Custom Lobby (Room #{Lobby.RoomId}). Parameters: Map={Lobby.SelectedMap}, Mode={Lobby.SelectedMode}, Slots={Lobby.MaxPlayers}. Clean launch without non-standard CLI arguments.");

        // Clean launch of ros.exe without unofficial CLI switches
        bool success = _launchService.LaunchGame(Installation, Settings, out string error);
        if (success)
        {
            LaunchStatus = "Game Active";
            StatusMessage = $"Rules of Survival launched. Notice: Official NetEase servers are offline. Lobby parameters (Map: {Lobby.SelectedMap}, Mode: {Lobby.SelectedMode}) are local launcher presets.";
            Profile.LastPlayedAt = DateTime.UtcNow;
            _ = _profileService.SaveProfileAsync(Profile);
        }
        else
        {
            LaunchStatus = "Ready";
            StatusMessage = error;
            _logger.LogWarning($"Start Match failed: {error}");
        }

        IsBusy = false;
    }

    private void OnBrowseGameDirectory()
    {
        try
        {
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
        OnPropertyChanged(nameof(CanStartMatch));
    }
}
