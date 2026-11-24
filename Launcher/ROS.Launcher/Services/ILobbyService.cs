using System.Collections.Generic;
using System.Threading.Tasks;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

/// <summary>
/// Interface for managing local custom lobbies.
/// Guarantees offline local persistence, atomic file updates, and safe capacity modifications.
/// </summary>
public interface ILobbyService
{
    string GetLobbyFilePath();
    Task<LobbyRoom> LoadOrCreateLobbyAsync(string hostName = "Guest");
    Task SaveLobbyAsync(LobbyRoom lobby);
    string GenerateUniqueRoomId(IEnumerable<string>? existingIds = null);
    CapacityChangeResult ChangeMode(LobbyRoom room, string newMode, bool confirmTruncate = false);
    bool AddBotSlot(LobbyRoom room, out string message);
    bool ClearSlot(LobbyRoom room, int slotNumber, out string message);
    void SyncHostName(LobbyRoom room, string hostName);
    LobbyRoom CreateDefaultLobby(string hostName = "Guest");
}
