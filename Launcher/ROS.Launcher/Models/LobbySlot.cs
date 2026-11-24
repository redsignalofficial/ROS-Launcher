namespace ROS.Launcher.Models;

/// <summary>
/// Represents a participant or placeholder slot in a local custom lobby.
/// Notice: Bot slots are visual / offline UI placeholders only.
/// The ROS launcher does not inject or simulate in-game AI bots into ros.exe.
/// </summary>
public class LobbySlot
{
    public int SlotNumber { get; set; } = 1;
    public string PlayerName { get; set; } = "Open Slot";
    public bool IsOccupied { get; set; }
    public bool IsHost { get; set; }
    public bool IsBot { get; set; }
    public bool IsReady { get; set; }
    public string Role { get; set; } = "Member";
    public string Note { get; set; } = "";

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
