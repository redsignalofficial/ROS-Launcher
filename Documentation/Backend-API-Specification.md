# ROS REVIVAL — Backend API Specification (v1.0)

## 1. Specification Overview

This document specifies the RESTful and real-time WebSocket/SignalR API contract for the **ROS REVIVAL Backend**.

- **Protocol:** HTTPS (REST) & WSS (WebSocket / SignalR)
- **Base URL:** `/api/v1`
- **Data Format:** UTF-8 `application/json`
- **Authentication:** `Authorization: Bearer <JWT_TOKEN>` (for protected endpoints)
- **Error Standard:** RFC 7807 Problem Details

---

## 2. Standard Response Envelope

All API endpoints return responses encapsulated in a uniform JSON envelope:

```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2026-11-24T13:20:00Z"
}
```

In the event of an error:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "CAPACITY_REDUCTION_WARNING",
    "message": "Reducing capacity to Duo would remove 1 occupied slot (#3).",
    "details": {
      "targetMode": "Duo",
      "occupiedSlotsAtRisk": [3]
    }
  },
  "timestamp": "2026-11-24T13:20:00Z"
}
```

---

## 3. Authentication & Guest Identity Endpoints

### 3.1 Exchange Guest Token
- **Endpoint:** `POST /api/v1/auth/guest`
- **Auth Required:** None (Public)
- **Description:** Generates a signed stateless JWT token for an offline guest user.
- **Request Body:**
  ```json
  {
    "localId": "e7fbf0551d8f",
    "nickname": "Survivor_2469",
    "clientVersion": "1.0.0"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "data": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "tokenType": "Bearer",
      "expiresIn": 86400,
      "player": {
        "playerId": "guest_e7fbf0551d8f",
        "nickname": "Survivor_2469",
        "role": "Guest"
      }
    }
  }
  ```

---

## 4. Configuration & Integrity Manifest Endpoints

### 4.1 Client Asset Manifest
- **Endpoint:** `GET /api/v1/config/manifest`
- **Auth Required:** None (Public)
- **Description:** Provides baseline SHA-256 cryptographic hashes for untouched ROS game client verification.
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "data": {
      "clientVersion": "1.9.0",
      "launcherMinimumVersion": "1.0.0",
      "requiredFiles": [
        {
          "relativePath": "Game/ros.exe",
          "sha256": "1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd",
          "sizeBytes": 3288480
        },
        {
          "relativePath": "Game/assets.npk",
          "sha256": "4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908",
          "sizeBytes": 184520934
        },
        {
          "relativePath": "Game/script.npk",
          "sha256": "c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852",
          "sizeBytes": 5420184
        }
      ]
    }
  }
  ```

### 4.2 Service Health & Notice
- **Endpoint:** `GET /api/v1/config/status`
- **Auth Required:** None (Public)
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "data": {
      "status": "OPERATIONAL",
      "officialServersStatus": "OFFLINE_PERMANENT",
      "communityBackendStatus": "ACTIVE_MOCK",
      "announcement": "Welcome to ROS Revival. Authentic Rules of Survival preservation."
    }
  }
  ```

---

## 5. Lobby Management Endpoints

### 5.1 Create Custom Room
- **Endpoint:** `POST /api/v1/lobby/create`
- **Auth Required:** Bearer Token
- **Request Body:**
  ```json
  {
    "roomName": "Community Battle",
    "selectedMap": "Ghillie Island",
    "selectedMode": "Squad",
    "perspective": "TPP"
  }
  ```
- **Response (201 Created):**
  ```json
  {
    "success": true,
    "data": {
      "roomId": "962725",
      "roomName": "Community Battle",
      "hostPlayerId": "guest_e7fbf0551d8f",
      "selectedMap": "Ghillie Island",
      "selectedMode": "Squad",
      "perspective": "TPP",
      "maxPlayers": 4,
      "occupiedCount": 1,
      "slots": [
        {
          "slotNumber": 1,
          "playerId": "guest_e7fbf0551d8f",
          "playerName": "Survivor_2469",
          "isOccupied": true,
          "isHost": true,
          "isBot": false,
          "isReady": true,
          "role": "Host"
        },
        { "slotNumber": 2, "isOccupied": false, "playerName": "Open Slot", "role": "Member" },
        { "slotNumber": 3, "isOccupied": false, "playerName": "Open Slot", "role": "Member" },
        { "slotNumber": 4, "isOccupied": false, "playerName": "Open Slot", "role": "Member" }
      ],
      "createdAt": "2026-11-24T13:20:00Z"
    }
  }
  ```

### 5.2 Get Room State
- **Endpoint:** `GET /api/v1/lobby/{roomId}`
- **Auth Required:** Bearer Token
- **Parameters:** `roomId` (Regex: `^\d{6}$`)
- **Response (200 OK):** Returns current room details.

### 5.3 Join Room
- **Endpoint:** `POST /api/v1/lobby/{roomId}/join`
- **Auth Required:** Bearer Token
- **Response (200 OK):** Returns updated room state and assigned `slotNumber`.

### 5.4 Leave Room
- **Endpoint:** `POST /api/v1/lobby/{roomId}/leave`
- **Auth Required:** Bearer Token
- **Response (200 OK):** Confirms departure; if host leaves, migrates host or disbands room.

### 5.5 Change Game Mode (Capacity Guarded)
- **Endpoint:** `PUT /api/v1/lobby/{roomId}/mode`
- **Auth Required:** Bearer Token (Host Only)
- **Request Body:**
  ```json
  {
    "newMode": "Duo",
    "confirmTruncate": false
  }
  ```
- **Response if Truncation Risk Exists (409 Conflict):**
  ```json
  {
    "success": false,
    "error": {
      "code": "CAPACITY_REDUCTION_WARNING",
      "message": "Reducing capacity to Duo would remove occupied slot(s): #3, #4.",
      "details": {
        "newCapacity": 2,
        "occupiedSlotsAtRisk": [3, 4]
      }
    }
  }
  ```
- **Response if Confirmed (`confirmTruncate: true`) (200 OK):**
  Returns updated room with capacity reduced to 2.

### 5.6 Add Visual Placeholder Bot
- **Endpoint:** `POST /api/v1/lobby/{roomId}/bots`
- **Auth Required:** Bearer Token (Host Only)
- **Response (200 OK):** Fills lowest open slot with visual placeholder bot.

### 5.7 Clear / Kick Slot
- **Endpoint:** `DELETE /api/v1/lobby/{roomId}/slots/{slotNumber}`
- **Auth Required:** Bearer Token (Host Only)
- **Rules:** Cannot kick Slot 1 (Host). Clears slot to Open.

---

## 6. Matchmaking Endpoints

### 6.1 Enter Queue
- **Endpoint:** `POST /api/v1/matchmaking/queue`
- **Auth Required:** Bearer Token
- **Request Body:**
  ```json
  {
    "selectedMap": "Fearless Fiord",
    "selectedMode": "Squad",
    "perspective": "TPP"
  }
  ```
- **Response (202 Accepted):** Returns `ticketId` and estimated queue wait time.

### 6.2 Matchmaking Ticket Status
- **Endpoint:** `GET /api/v1/matchmaking/status?ticketId={ticketId}`
- **Auth Required:** Bearer Token
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "data": {
      "ticketId": "mm_ticket_894120",
      "status": "MATCHED",
      "matchSessionId": "sess_9918274"
    }
  }
  ```

---

## 7. Real-Time SignalR Hub Specification

- **Hub URL:** `/hubs/lobby`
- **Transport:** WebSockets (fallback: Server-Sent Events, Long Polling)
- **Protocol:** MessagePack or JSON

### Client-to-Server Methods:
| Method | Arguments | Description |
| :--- | :--- | :--- |
| `JoinLobbyGroup` | `string roomId` | Subscribes connection to room update broadcasts. |
| `LeaveLobbyGroup` | `string roomId` | Unsubscribes connection from room group. |
| `ToggleReady` | `string roomId, bool isReady` | Toggles player ready state. |

### Server-to-Client Events:
| Event Name | Payload | Description |
| :--- | :--- | :--- |
| `OnLobbyUpdated` | `LobbyRoomDTO` | Broadcast whenever map, mode, or slots change. |
| `OnMemberJoined` | `LobbySlotDTO` | Broadcast when a new player joins an open slot. |
| `OnMemberLeft` | `int slotNumber` | Broadcast when a player leaves or is cleared. |
| `OnCapacityWarning` | `WarningDTO` | Sent to host when downsizing requires confirmation. |
| `OnMatchStarting` | `SessionInfoDTO` | Broadcast when match starts with session details. |

---

## 8. Error Codes Reference

| Error Code | HTTP Status | Description |
| :--- | :---: | :--- |
| `ROOM_NOT_FOUND` | 404 | Specified 6-digit room does not exist or expired. |
| `ROOM_FULL` | 409 | All player slots for current mode are occupied. |
| `NOT_ROOM_HOST` | 403 | Caller does not possess host privileges for this action. |
| `CANNOT_REMOVE_HOST` | 400 | Attempted to delete or kick Slot 1 (Host). |
| `CAPACITY_REDUCTION_WARNING`| 409 | Mode downsize would truncate occupied slots without confirmation. |
| `INVALID_ROOM_ID` | 400 | Room ID format must be exactly 6 numeric digits. |
| `UNAUTHORIZED` | 401 | Missing or invalid JWT session token. |
| `RATE_LIMITED` | 429 | Too many requests; retry after backoff interval. |
