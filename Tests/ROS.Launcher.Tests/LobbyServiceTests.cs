using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ROS.Launcher.Models;
using ROS.Launcher.Services;
using Xunit;

namespace ROS.Launcher.Tests;

public class LobbyServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly ILauncherLogService _logger;

    public LobbyServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ROSTests_Lobby_" + Guid.NewGuid().ToString("N"));
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
    public void GenerateUniqueRoomId_ReturnsValidSixDigitNumericString()
    {
        var service = new LocalLobbyService(_logger, _testDir);

        for (int i = 0; i < 50; i++)
        {
            string roomId = service.GenerateUniqueRoomId();

            Assert.Equal(6, roomId.Length);
            Assert.True(int.TryParse(roomId, out int numericId));
            Assert.InRange(numericId, 100000, 999999);
        }
    }

    [Fact]
    public void GenerateUniqueRoomId_AvoidsCollisionsWithExistingIds()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var existing = new HashSet<string> { "123456", "654321", "100001" };

        string generated = service.GenerateUniqueRoomId(existing);

        Assert.Equal(6, generated.Length);
        Assert.DoesNotContain(generated, existing);
    }

    [Fact]
    public void GenerateUniqueRoomId_MassGeneration_ProducesHighUniqueness()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var generatedIds = new HashSet<string>();

        for (int i = 0; i < 500; i++)
        {
            string id = service.GenerateUniqueRoomId(generatedIds);
            bool added = generatedIds.Add(id);
            Assert.True(added, $"Encountered collision on ID: {id}");
        }

        Assert.Equal(500, generatedIds.Count);
    }

    [Fact]
    public void CreateDefaultLobby_SetsCorrectDefaultsAndHost()
    {
        var service = new LocalLobbyService(_logger, _testDir);

        var room = service.CreateDefaultLobby("TestLeader");

        Assert.NotNull(room);
        Assert.Equal(6, room.RoomId.Length);
        Assert.Equal("Ghillie Island", room.SelectedMap);
        Assert.Equal("Squad", room.SelectedMode);
        Assert.Equal("TPP", room.Perspective);
        Assert.Equal(4, room.MaxPlayers);
        Assert.Equal(4, room.Slots.Count);

        // Slot 1 is host
        var hostSlot = room.Slots[0];
        Assert.Equal(1, hostSlot.SlotNumber);
        Assert.Equal("TestLeader", hostSlot.PlayerName);
        Assert.True(hostSlot.IsHost);
        Assert.True(hostSlot.IsOccupied);
        Assert.False(hostSlot.IsBot);

        // Slots 2-4 are open
        for (int i = 1; i < 4; i++)
        {
            Assert.Equal(i + 1, room.Slots[i].SlotNumber);
            Assert.False(room.Slots[i].IsOccupied);
            Assert.False(room.Slots[i].IsBot);
        }
    }

    [Fact]
    public void AddBotSlot_FillsOpenSlotWithVisualPlaceholderBot()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("Commander");

        bool success = service.AddBotSlot(room, out string message);

        Assert.True(success);
        Assert.Contains("Slot 2", message);
        Assert.Contains("placeholder", message.ToLowerInvariant());

        var botSlot = room.Slots[1];
        Assert.Equal(2, botSlot.SlotNumber);
        Assert.True(botSlot.IsOccupied);
        Assert.True(botSlot.IsBot);
        Assert.False(botSlot.IsHost);
        Assert.Contains("Visual Placeholder", botSlot.Role);
        Assert.Contains("placeholder", botSlot.Note.ToLowerInvariant());
    }

    [Fact]
    public void AddBotSlot_WhenLobbyIsFull_ReturnsFalseAndWarns()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer");

        // Fill slots 2, 3, 4 with bots
        Assert.True(service.AddBotSlot(room, out _));
        Assert.True(service.AddBotSlot(room, out _));
        Assert.True(service.AddBotSlot(room, out _));

        // Attempt to add a 5th member in a 4-slot room
        bool fifthAdded = service.AddBotSlot(room, out string warnMessage);

        Assert.False(fifthAdded);
        Assert.Contains("occupied", warnMessage.ToLowerInvariant());
    }

    [Fact]
    public void ClearSlot_ResetsBotSlotToOpen()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer");

        service.AddBotSlot(room, out _);
        Assert.True(room.Slots[1].IsOccupied);

        bool cleared = service.ClearSlot(room, 2, out string clearMsg);

        Assert.True(cleared);
        Assert.Contains("cleared", clearMsg.ToLowerInvariant());
        Assert.False(room.Slots[1].IsOccupied);
        Assert.False(room.Slots[1].IsBot);
    }

    [Fact]
    public void ClearSlot_ProtectsHostInSlot1_ReturnsFalse()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer");

        bool hostCleared = service.ClearSlot(room, 1, out string message);

        Assert.False(hostCleared);
        Assert.Contains("cannot remove", message.ToLowerInvariant());
        Assert.True(room.Slots[0].IsHost);
        Assert.True(room.Slots[0].IsOccupied);
    }

    [Fact]
    public void ChangeMode_ExpandsSlots_WhenMovingToHigherCapacity()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("SoloPlayer");
        room.Slots = new System.Collections.ObjectModel.ObservableCollection<LobbySlot> { LobbySlot.CreateHost("SoloPlayer") };
        room.SelectedMode = "Solo";
        room.MaxPlayers = 1;

        var result = service.ChangeMode(room, "Squad", confirmTruncate: false);

        Assert.Equal(CapacityChangeStatus.Success, result.Status);
        Assert.Equal(4, room.MaxPlayers);
        Assert.Equal(4, room.Slots.Count);
        Assert.Equal("Squad", room.SelectedMode);
        Assert.False(room.Slots[1].IsOccupied);
        Assert.False(room.Slots[2].IsOccupied);
        Assert.False(room.Slots[3].IsOccupied);
    }

    [Fact]
    public void ChangeMode_TruncatesSafely_WhenExcessSlotsAreOpen()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer"); // 4 slots, 2-4 open

        var result = service.ChangeMode(room, "Duo", confirmTruncate: false);

        Assert.Equal(CapacityChangeStatus.Success, result.Status);
        Assert.Equal(2, room.MaxPlayers);
        Assert.Equal(2, room.Slots.Count);
        Assert.Equal("Duo", room.SelectedMode);
        Assert.Equal(0, result.OccupiedSlotsAtRisk);
    }

    [Fact]
    public void ChangeMode_WarnsAndPreservesData_WhenOccupiedSlotsWouldBeRemoved()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer");

        // Occupy slots 3 and 4 with bots
        room.Slots[2] = LobbySlot.CreateBot(3);
        room.Slots[3] = LobbySlot.CreateBot(4);

        // Attempt to reduce capacity from 4 to 2 (Duo) without confirmation
        var result = service.ChangeMode(room, "Duo", confirmTruncate: false);

        Assert.Equal(CapacityChangeStatus.WarningOccupiedSlotsWouldBeRemoved, result.Status);
        Assert.Equal(2, result.OccupiedSlotsAtRisk);
        Assert.Contains(3, result.TruncatedSlotNumbers);
        Assert.Contains(4, result.TruncatedSlotNumbers);

        // Ensure lobby was NOT modified (zero data loss)
        Assert.Equal(4, room.Slots.Count);
        Assert.Equal("Squad", room.SelectedMode);
        Assert.True(room.Slots[2].IsOccupied);
        Assert.True(room.Slots[3].IsOccupied);
    }

    [Fact]
    public void ChangeMode_TruncatesExcessSlots_WhenConfirmedByUser()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("HostPlayer");

        // Occupy slot 3 with bot
        room.Slots[2] = LobbySlot.CreateBot(3);

        // Confirm truncation
        var result = service.ChangeMode(room, "Duo", confirmTruncate: true);

        Assert.Equal(CapacityChangeStatus.Success, result.Status);
        Assert.Equal(2, room.MaxPlayers);
        Assert.Equal(2, room.Slots.Count);
        Assert.Equal("Duo", room.SelectedMode);
        Assert.True(room.Slots[0].IsHost);
    }

    [Fact]
    public async Task SaveLobbyAsync_WritesAtomically_AndCleansUpTempFiles()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("AtomicHost");

        await service.SaveLobbyAsync(room);

        string filePath = service.GetLobbyFilePath();
        Assert.True(File.Exists(filePath));

        // Verify no leftover .tmp files
        var tempFiles = Directory.GetFiles(_testDir, "*.tmp*");
        Assert.Empty(tempFiles);
    }

    [Fact]
    public async Task LoadOrCreateLobbyAsync_RecoversFromCorruptedJson_AndCreatesBackup()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        string lobbyPath = service.GetLobbyFilePath();

        // Write malformed, corrupted JSON
        await File.WriteAllTextAsync(lobbyPath, "{ \"RoomId\": \"999999\", \"BrokenJson\": [unclosed");

        // Load must not crash or throw unhandled exception
        var recoveredRoom = await service.LoadOrCreateLobbyAsync("RecoveredHost");

        Assert.NotNull(recoveredRoom);
        Assert.Equal(6, recoveredRoom.RoomId.Length);
        Assert.Equal("RecoveredHost", recoveredRoom.Slots[0].PlayerName);

        // Verify a backup copy was created
        var backupFiles = Directory.GetFiles(_testDir, "*lobby.json.corrupted*");
        Assert.NotEmpty(backupFiles);
    }

    [Fact]
    public async Task LobbyPersistence_RoundTrip_PreservesAllLobbySettings()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("CustomUser");

        room.RoomId = "554433";
        room.RoomName = "Custom Tournament Match";
        room.SelectedMap = "Fearless Fiord";
        service.ChangeMode(room, "Fireteam");
        room.Perspective = "FPP";
        service.AddBotSlot(room, out _);

        await service.SaveLobbyAsync(room);

        // Create new service instance pointing to the same folder
        var freshService = new LocalLobbyService(_logger, _testDir);
        var loaded = await freshService.LoadOrCreateLobbyAsync("CustomUser");

        Assert.Equal("554433", loaded.RoomId);
        Assert.Equal("Custom Tournament Match", loaded.RoomName);
        Assert.Equal("Fearless Fiord", loaded.SelectedMap);
        Assert.Equal("Fireteam", loaded.SelectedMode);
        Assert.Equal("FPP", loaded.Perspective);
        Assert.Equal(5, loaded.MaxPlayers);
        Assert.Equal(5, loaded.Slots.Count);
        Assert.Equal(1, loaded.BotCount);
    }

    [Theory]
    [InlineData("Solo", 1)]
    [InlineData("Duo", 2)]
    [InlineData("Squad", 4)]
    [InlineData("Fireteam", 5)]
    public void ChangeMode_AlwaysSetsExactModeAndCapacityInLockstep(string targetMode, int expectedCapacity)
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("TestHost");

        var result = service.ChangeMode(room, targetMode, confirmTruncate: true);

        Assert.Equal(CapacityChangeStatus.Success, result.Status);
        Assert.Equal(targetMode, room.SelectedMode);
        Assert.Equal(expectedCapacity, room.MaxPlayers);
        Assert.Equal(expectedCapacity, room.Slots.Count);
        Assert.Equal(expectedCapacity, LobbyRoom.GetModeCapacity(targetMode));
    }

    [Fact]
    public void LobbyRoom_PerspectiveAndMap_MaintainStateConsistency()
    {
        var service = new LocalLobbyService(_logger, _testDir);
        var room = service.CreateDefaultLobby("TestHost");

        // Verify default state
        Assert.Equal("Ghillie Island", room.SelectedMap);
        Assert.Equal("Squad", room.SelectedMode);
        Assert.Equal("TPP", room.Perspective);
        Assert.Equal(4, room.MaxPlayers);

        // Transition Map
        room.SelectedMap = "Fearless Fiord";
        Assert.Equal("Fearless Fiord", room.SelectedMap);

        // Transition Perspective
        room.Perspective = "FPP";
        Assert.Equal("FPP", room.Perspective);

        // Transition Mode to Duo
        service.ChangeMode(room, "Duo", confirmTruncate: true);
        Assert.Equal("Duo", room.SelectedMode);
        Assert.Equal(2, room.MaxPlayers);
        Assert.Equal(2, room.Slots.Count);

        // Summary representation
        string summary = $"Map: {room.SelectedMap}  •  Mode: {room.SelectedMode} ({room.MaxPlayers}P)  •  Perspective: {room.Perspective}  •  Slots: {room.OccupiedCount}/{room.MaxPlayers}";
        Assert.Equal("Map: Fearless Fiord  •  Mode: Duo (2P)  •  Perspective: FPP  •  Slots: 1/2", summary);
    }
}
