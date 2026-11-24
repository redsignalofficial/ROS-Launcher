using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ROS.Launcher.Models;

/// <summary>
/// Represents a participant or placeholder slot in a local custom lobby.
/// Notice: Bot slots are visual / offline UI placeholders only.
/// The ROS launcher does not inject or simulate in-game AI bots into ros.exe.
/// </summary>
public class LobbySlot : INotifyPropertyChanged
{
    private int _slotNumber = 1;
    private string _playerName = "Open Slot";
    private bool _isOccupied;
    private bool _isHost;
    private bool _isBot;
    private bool _isReady;
    private string _role = "Member";
    private string _note = "";

    public int SlotNumber
    {
        get => _slotNumber;
        set => SetField(ref _slotNumber, value);
    }

    public string PlayerName
    {
        get => _playerName;
        set => SetField(ref _playerName, value);
    }

    public bool IsOccupied
    {
        get => _isOccupied;
        set
        {
            if (SetField(ref _isOccupied, value))
            {
                OnPropertyChanged(nameof(CanClear));
            }
        }
    }

    public bool IsHost
    {
        get => _isHost;
        set
        {
            if (SetField(ref _isHost, value))
            {
                OnPropertyChanged(nameof(CanClear));
            }
        }
    }

    public bool IsBot
    {
        get => _isBot;
        set => SetField(ref _isBot, value);
    }

    public bool IsReady
    {
        get => _isReady;
        set => SetField(ref _isReady, value);
    }

    public string Role
    {
        get => _role;
        set => SetField(ref _role, value);
    }

    public string Note
    {
        get => _note;
        set => SetField(ref _note, value);
    }

    /// <summary>
    /// Only occupied non-host slots can be cleared.
    /// Open slots and the room host cannot be cleared.
    /// </summary>
    public bool CanClear => IsOccupied && !IsHost;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    public static LobbySlot CreateHost(string hostName) => new()
    {
        SlotNumber = 1,
        PlayerName = string.IsNullOrWhiteSpace(hostName) ? "GuestHost" : hostName,
        IsOccupied = true,
        IsHost = true,
        IsBot = false,
        IsReady = true,
        Role = "Host",
        Note = "Local Room Leader"
    };

    public static LobbySlot CreateOpen(int slotNumber) => new()
    {
        SlotNumber = slotNumber,
        PlayerName = $"Slot {slotNumber} (Open)",
        IsOccupied = false,
        IsHost = false,
        IsBot = false,
        IsReady = false,
        Role = "Member",
        Note = "Available Slot"
    };

    public static LobbySlot CreateBot(int slotNumber, string? botName = null) => new()
    {
        SlotNumber = slotNumber,
        PlayerName = botName ?? $"Training Bot #{slotNumber:D2}",
        IsOccupied = true,
        IsHost = false,
        IsBot = true,
        IsReady = true,
        Role = "Visual Placeholder",
        Note = "Offline UI placeholder only (No in-game AI)"
    };
}
