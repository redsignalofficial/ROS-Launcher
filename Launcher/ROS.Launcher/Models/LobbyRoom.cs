using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ROS.Launcher.Models;

/// <summary>
/// Represents a local-only Custom Lobby room configuration.
/// Room IDs are strictly 6-digit local identifiers for offline management.
/// Match parameters are stored locally and not transmitted over any network.
/// </summary>
public class LobbyRoom
{
    public string RoomId { get; set; } = "100001";
    public string RoomName { get; set; } = "Local Custom Match";
    public string SelectedMap { get; set; } = "Ghillie Island";
    public string SelectedMode { get; set; } = "Squad"; // Solo, Duo, Squad, Fireteam
    public string Perspective { get; set; } = "TPP"; // TPP, FPP
    public int MaxPlayers { get; set; } = 4;
    public ObservableCollection<LobbySlot> Slots { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public static int GetModeCapacity(string mode) => mode switch
    {
        "Solo" => 1,
        "Duo" => 2,
        "Squad" => 4,
        "Fireteam" => 5,
        _ => 4
    };

    public int OccupiedCount => Slots.Count(s => s.IsOccupied);
    public int BotCount => Slots.Count(s => s.IsBot);
    public int OpenSlotCount => Slots.Count(s => !s.IsOccupied);
}
