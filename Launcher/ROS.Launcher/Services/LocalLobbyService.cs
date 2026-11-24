using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

/// <summary>
/// Handles offline local custom lobby operations.
/// Features:
/// - 6-digit numeric room IDs strictly for offline local identification.
/// - Atomic file writes to prevent corrupted or partial file states.
/// - Graceful corruption recovery with automatic backup.
/// - Capacity change protection with user warnings before occupied slots are removed.
/// - Visual placeholder bot slots (clearly documented as non-AI offline placeholders).
/// </summary>
public class LocalLobbyService : ILobbyService
{
    private readonly string _lobbyFilePath;
    private readonly ILauncherLogService _logger;

    public LocalLobbyService(ILauncherLogService? logger = null, string? customBasePath = null)
    {
        _logger = logger ?? new LauncherLogService();
        string basePath = customBasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ROSRevival"
        );

        Directory.CreateDirectory(basePath);
        _lobbyFilePath = Path.Combine(basePath, "lobby.json");
    }

    public string GetLobbyFilePath() => _lobbyFilePath;

    public async Task<LobbyRoom> LoadOrCreateLobbyAsync(string hostName = "Guest")
    {
        if (File.Exists(_lobbyFilePath))
        {
            try
            {
                string json = await File.ReadAllTextAsync(_lobbyFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var room = JsonSerializer.Deserialize<LobbyRoom>(json);
                    if (room != null && !string.IsNullOrWhiteSpace(room.RoomId) && room.Slots != null && room.Slots.Count > 0)
                    {
                        SyncHostName(room, hostName);
                        _logger.LogInfo($"Loaded custom lobby configuration for Room #{room.RoomId} ({room.SelectedMode}).");
                        return room;
                    }
                }

                _logger.LogWarning($"Lobby configuration file at '{_lobbyFilePath}' was invalid or incomplete. Creating default.");
            }
            catch (JsonException jex)
            {
                _logger.LogWarning($"Lobby configuration file is corrupted JSON: {jex.Message}. Backing up and restoring defaults.");
                BackupCorruptedFile();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to read lobby file at '{_lobbyFilePath}'. Backing up and restoring defaults.", ex);
                BackupCorruptedFile();
            }
        }

        var defaultLobby = CreateDefaultLobby(hostName);
        await SaveLobbyAsync(defaultLobby);
        return defaultLobby;
    }

    public async Task SaveLobbyAsync(LobbyRoom lobby)
    {
        lobby.LastModified = DateTime.UtcNow;
        string json = JsonSerializer.Serialize(lobby, new JsonSerializerOptions { WriteIndented = true });

        string dir = Path.GetDirectoryName(_lobbyFilePath) ?? AppContext.BaseDirectory;
        string tempFilePath = Path.Combine(dir, $"{Path.GetFileName(_lobbyFilePath)}.tmp.{Guid.NewGuid():N}");

        try
        {
            // Write atomically to temporary file, then move to overwrite target
            await File.WriteAllTextAsync(tempFilePath, json);
            File.Move(tempFilePath, _lobbyFilePath, overwrite: true);
            _logger.LogInfo($"Lobby configuration saved atomically to '{_lobbyFilePath}'. Room #{lobby.RoomId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save lobby configuration to '{_lobbyFilePath}'.", ex);
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { /* ignore cleanup error */ }
            }
            throw;
        }
    }

    public string GenerateUniqueRoomId(IEnumerable<string>? existingIds = null)
    {
        var existingSet = existingIds != null ? new HashSet<string>(existingIds) : null;
        for (int attempts = 0; attempts < 1000; attempts++)
        {
            int num = Random.Shared.Next(100000, 1000000); // 100000 to 999999 inclusive
            string id = num.ToString("D6");
            if (existingSet == null || !existingSet.Contains(id))
            {
                return id;
            }
        }

        return (100000 + (Math.Abs(Guid.NewGuid().GetHashCode()) % 900000)).ToString("D6");
    }

    public CapacityChangeResult ChangeMode(LobbyRoom room, string newMode, bool confirmTruncate = false)
    {
        int targetCapacity = LobbyRoom.GetModeCapacity(newMode);
        int currentCapacity = room.Slots.Count;

        if (currentCapacity > targetCapacity)
        {
            var slotsAtRisk = room.Slots
                .Where(s => s.SlotNumber > targetCapacity && s.IsOccupied)
                .ToList();

            if (slotsAtRisk.Count > 0 && !confirmTruncate)
            {
                return new CapacityChangeResult
                {
                    Status = CapacityChangeStatus.WarningOccupiedSlotsWouldBeRemoved,
                    NewCapacity = targetCapacity,
                    OccupiedSlotsAtRisk = slotsAtRisk.Count,
                    TruncatedSlotNumbers = slotsAtRisk.Select(s => s.SlotNumber).ToList(),
                    Message = $"Reducing capacity to {newMode} ({targetCapacity} players) would remove occupied slot(s): #{string.Join(", #", slotsAtRisk.Select(s => s.SlotNumber))}. Confirm to proceed."
                };
            }

            // Truncate safely, preserving remaining slots
            room.Slots = room.Slots.Take(targetCapacity).ToList();
            room.MaxPlayers = targetCapacity;
            room.SelectedMode = newMode;
            room.LastModified = DateTime.UtcNow;

            return new CapacityChangeResult
            {
                Status = CapacityChangeStatus.Success,
                NewCapacity = targetCapacity,
                OccupiedSlotsAtRisk = slotsAtRisk.Count,
                TruncatedSlotNumbers = slotsAtRisk.Select(s => s.SlotNumber).ToList(),
                Message = $"Mode changed to {newMode} ({targetCapacity} players). Excess slots removed."
            };
        }
        else if (currentCapacity < targetCapacity)
        {
            // Expand slots up to target capacity
            for (int i = currentCapacity + 1; i <= targetCapacity; i++)
            {
                room.Slots.Add(LobbySlot.CreateOpen(i));
            }

            room.MaxPlayers = targetCapacity;
            room.SelectedMode = newMode;
            room.LastModified = DateTime.UtcNow;

            return new CapacityChangeResult
            {
                Status = CapacityChangeStatus.Success,
                NewCapacity = targetCapacity,
                OccupiedSlotsAtRisk = 0,
                Message = $"Mode changed to {newMode} ({targetCapacity} players)."
            };
        }
        else
        {
            room.SelectedMode = newMode;
            return new CapacityChangeResult
            {
                Status = CapacityChangeStatus.Success,
                NewCapacity = targetCapacity,
                OccupiedSlotsAtRisk = 0,
                Message = $"Mode set to {newMode}."
            };
        }
    }

    public bool AddBotSlot(LobbyRoom room, out string message)
    {
        var openSlot = room.Slots.FirstOrDefault(s => !s.IsOccupied);
        if (openSlot == null)
        {
            message = "All lobby slots are already occupied. Increase team size to add more slots.";
            return false;
        }

        int slotNum = openSlot.SlotNumber;
        int index = room.Slots.IndexOf(openSlot);
        room.Slots[index] = LobbySlot.CreateBot(slotNum);
        room.LastModified = DateTime.UtcNow;

        message = $"Added visual placeholder bot to Slot {slotNum}. Notice: Offline UI placeholder only (no in-game AI).";
        return true;
    }

    public bool ClearSlot(LobbyRoom room, int slotNumber, out string message)
    {
        if (slotNumber == 1)
        {
            message = "Cannot remove the Room Host (Slot 1).";
            return false;
        }

        var slot = room.Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot == null)
        {
            message = $"Slot #{slotNumber} does not exist in this lobby.";
            return false;
        }

        if (!slot.IsOccupied)
        {
            message = $"Slot #{slotNumber} is already open.";
            return false;
        }

        int index = room.Slots.IndexOf(slot);
        room.Slots[index] = LobbySlot.CreateOpen(slotNumber);
        room.LastModified = DateTime.UtcNow;

        message = $"Slot #{slotNumber} cleared to Open.";
        return true;
    }

    public void SyncHostName(LobbyRoom room, string hostName)
    {
        string name = string.IsNullOrWhiteSpace(hostName) ? "GuestHost" : hostName;
        if (room.Slots.Count > 0)
        {
            room.Slots[0].PlayerName = name;
            room.Slots[0].IsHost = true;
            room.Slots[0].IsOccupied = true;
            room.Slots[0].Role = "Host";
        }
        else
        {
            room.Slots.Add(LobbySlot.CreateHost(name));
        }
    }

    public LobbyRoom CreateDefaultLobby(string hostName = "Guest")
    {
        string host = string.IsNullOrWhiteSpace(hostName) ? "Guest" : hostName;
        var room = new LobbyRoom
        {
            RoomId = GenerateUniqueRoomId(),
            RoomName = $"{host}'s Custom Lobby",
            SelectedMap = "Ghillie Island",
            SelectedMode = "Squad",
            Perspective = "TPP",
            MaxPlayers = 4,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            Slots = new List<LobbySlot>
            {
                LobbySlot.CreateHost(host),
                LobbySlot.CreateOpen(2),
                LobbySlot.CreateOpen(3),
                LobbySlot.CreateOpen(4)
            }
        };

        return room;
    }

    private void BackupCorruptedFile()
    {
        try
        {
            if (File.Exists(_lobbyFilePath))
            {
                string backupPath = $"{_lobbyFilePath}.corrupted.{DateTime.UtcNow:yyyyMMdd_HHmmss}";
                File.Copy(_lobbyFilePath, backupPath, overwrite: true);
                _logger.LogInfo($"Corrupted lobby file safely backed up to '{backupPath}'.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create backup copy of corrupted lobby file.", ex);
        }
    }
}
