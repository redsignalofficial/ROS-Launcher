# ROS REVIVAL — Proposed Local Mock Backend Design

## 1. Overview

The **Local Mock Backend** (`ROS.Revival.MockBackend`) is a lightweight, self-contained ASP.NET Core service designed to provide a fully functional local development and testing environment for the ROS Revival Launcher.

It allows end-to-end testing of:
- Guest authentication & JWT token issuance
- Remote custom room creation & 6-digit ID routing
- Multi-client real-time lobby synchronization via WebSockets (SignalR)
- Team mode capacity protection over the network
- Client manifest and status distribution

**Key Principle**: Operates on `localhost:5000` with **$0.00 cost**, zero cloud deployment, zero external database requirements, and zero modification of original ROS game files.

---

## 2. Solution Structure

```
ROS-REVIVAL/
├── Backend/
│   └── ROS.Revival.MockBackend/
│       ├── Program.cs                  # ASP.NET Core Minimal APIs & SignalR configuration
│       ├── Hubs/
│       │   └── LobbyHub.cs             # Real-time WebSocket room broadcast hub
│       ├── Endpoints/
│       │   ├── AuthEndpoints.cs        # POST /api/v1/auth/guest
│       │   ├── LobbyEndpoints.cs       # Room CRUD, joins, capacity changes
│       │   └── ConfigEndpoints.cs      # Manifests, version checks, healthz
│       ├── Services/
│       │   ├── IRoomManager.cs         # In-memory room store interface
│       │   └── InMemoryRoomManager.cs  # ConcurrentDictionary room store with TTL cleanup
│       └── ROS.Revival.MockBackend.csproj
├── Launcher/
│   └── ROS.Launcher/                  # WPF Client (Existing)
└── Tests/
    └── ROS.Revival.MockBackend.Tests/ # Automated integration tests using WebApplicationFactory
```

---

## 3. Core Implementation Architecture

### 3.1 Project File (`ROS.Revival.MockBackend.csproj`)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFrameworks>net10.0;net8.0</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>ROS.Revival.MockBackend</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### 3.2 In-Memory Room Store (`InMemoryRoomManager.cs`)
- Uses `ConcurrentDictionary<string, LobbyRoom>` for thread-safe state access.
- Automatically cleans up inactive rooms after 2 hours of inactivity.
- Enforces unique 6-digit local room IDs and capacity reduction safeguards.

### 3.3 Real-Time WebSocket Hub (`LobbyHub.cs`)
```csharp
public class LobbyHub : Hub
{
    private readonly IRoomManager _roomManager;

    public LobbyHub(IRoomManager roomManager) => _roomManager = roomManager;

    public async Task JoinLobbyGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
        var room = _roomManager.GetRoom(roomId);
        if (room != null)
        {
            await Clients.Caller.SendAsync("OnLobbyUpdated", room);
        }
    }

    public async Task LeaveLobbyGroup(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
    }
}
```

---

## 4. Verification & Testing Workflow

1. **Launch Mock Backend**:
   ```bash
   dotnet run --project Backend/ROS.Revival.MockBackend
   # Server listening on http://127.0.0.1:5000
   ```

2. **Automated Integration Tests (`WebApplicationFactory`)**:
   - Executes full HTTP requests and WebSocket connections in-memory during CI/CD.
   - Tests room creation, concurrent joins, capacity warnings, and disconnects without opening external ports.

3. **Launcher Integration (Milestone 5)**:
   - Launcher can toggle between `Offline (Local JSON)` and `Connected (Local Mock Backend)` via launcher settings.
   - If the backend is offline, the launcher gracefully defaults to offline guest mode.
