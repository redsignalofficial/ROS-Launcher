# ROS REVIVAL — CLOUD WINDOWS GPU TESTING PLAN

**Project:** ROS REVIVAL (Rules of Survival Preservation Project)  
**Objective:** Execute Real Rules of Survival (`ros.exe`) on a Cloud Windows GPU Environment without Physical PC Hardware  
**Document Version:** 1.0.0  
**Status:** PROPOSED — PENDING DEVELOPER APPROVAL  

---

## 1. Executive Summary & Problem Statement

### The Problem
- The developer manages the project from a mobile device and does not have a physical Windows PC.
- Standard GitHub-hosted runners (`windows-latest`) run in a headless background service container (Session 0) with a software virtual display driver (`Microsoft Basic Display Driver`), lacking the physical 3D GPU and DirectX hardware context required by the NeoX game engine (`ros.exe`).
- Test 1 is currently in status **`TEST_BLOCKED`** because executing `ros.exe` requires:
  1. A 64-bit Windows operating system (Windows 10, 11, or Windows Server with Desktop Experience).
  2. A dedicated physical/virtualized GPU (NVIDIA, AMD, or Intel Iris) with DirectX 9.0c / DirectX 11 hardware acceleration.
  3. An active interactive user desktop session (Session 1) capable of window rendering.
  4. The authorized ~6.0 GB Rules of Survival game client files (`ros.exe`, `assets.npk`, `script.npk`).

### The Solution
Provision a **temporary, on-demand Windows GPU Cloud Virtual Machine** that registers as a GitHub Actions self-hosted runner with labels:
```
self-hosted, Windows, ROS-Test, Interactive, GPU
```
This machine runs only during test execution (15–30 minutes per run), executes the real `ros.exe` process, captures authentic 1080p PNG screenshots and MP4 video recordings, and uploads the evidence directly back to GitHub Actions as downloadable artifacts.

> [!IMPORTANT]
> **Zero Out-of-Pocket Commitment Policy**
> No cloud instance will be purchased, rented, or activated without your explicit review and approval. Multiple free-tier and low-cost options are detailed below.

---

## 2. Cloud Provider Comparison & Cost Analysis

| Provider / Service | GPU Hardware | OS & Desktop Session | Free Credits / Trial | Hourly Cost | Estimated Cost Per Test Run (30 min) | Monthly 24/7 (Reference) |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Google Cloud Platform (GCP)** *(Recommended for $0)* | NVIDIA Tesla T4 (16GB VRAM) | Windows Server 2022 + NVIDIA GRID (Interactive Session 1) | **$300 Free Credit** (90 days) | ~$0.55 / hr | **$0.00** *(Deducted from free credit)* | ~$390 / mo |
| **Microsoft Azure** *(Recommended for $0)* | NVIDIA A10 (NVadsA10v5) or Tesla M60 | Windows 10/11 Multi-Session or Server 2022 | **$200 Free Credit** (30 days) | ~$0.75 / hr | **$0.00** *(Deducted from free credit)* | ~$540 / mo |
| **AWS EC2 (Spot Instance)** *(Lowest Pay-As-You-Go)* | NVIDIA Tesla T4 (`g4dn.xlarge`, 4 vCPU, 16GB RAM) | Windows Server 2022 with Desktop Experience | AWS Free Tier (Standard credits) | **~$0.35 – $0.45 / hr** | **~$0.18 – $0.23** | ~$250 / mo |
| **AirGPU** *(Gaming-Optimized Pay-As-You-Go)* | NVIDIA Tesla T4 / L4 (GTX 1080 / RTX class) | Windows 10/11 Desktop + Parsec / RDP | Credit-based ($10 min deposit) | ~$0.65 – $0.90 / hr | **~$0.33 – $0.45** | N/A (Pay-per-hour) + $3.50/mo storage |
| **Shadow PC** *(Dedicated Monthly Cloud PC)* | NVIDIA RTX 2000 Ada / RTX 3070 equivalent | Full Windows 11 Home/Pro Desktop (Mobile iOS/Android App) | None | N/A (Flat subscription) | Included in flat fee | **$29.99 – $34.99 / mo** |

### Provider Evaluation & Verdict
1. **Best Zero-Cost Option ($0):** **Google Cloud Platform (GCP)** or **Microsoft Azure**.
   - Creating a new GCP account provides **$300 in free trial credits** valid for 90 days.
   - We can spin up a Windows Server VM with an NVIDIA Tesla T4 GPU, run our tests, collect visual evidence, and shut it down.
   - **Total cash cost to you: $0.00.**
2. **Best Ultra-Low Cost Option (No Monthly Commitment):** **AWS EC2 `g4dn.xlarge` Spot Instance**.
   - Hourly spot price for a Windows GPU instance is only **~$0.35/hour**.
   - Running the runner for 1 hour to conduct full testing costs approximately **35 cents**.
3. **Best Mobile-Interactive Option:** **Shadow PC ($29.99/mo)**.
   - Comes with a native mobile app for Android and iOS.
   - Allows you to open the actual Windows desktop on your phone, see the game window live on your mobile screen, and touch-interact with it.

---

## 3. Technical Architecture: Interactive Session & GPU Acceleration

For a Windows cloud VM to run a 3D DirectX game like Rules of Survival and capture genuine visual evidence, it must be configured for **Interactive Desktop Session Execution**:

```mermaid
flowchart TD
    subgraph GitHub Cloud
        GH[GitHub Actions Workflow Dispatch]
        Artifacts[GitHub Actions Artifacts: Screenshots & Videos]
    end

    subgraph Cloud Windows GPU VM (GCP / Azure / AWS / AirGPU)
        AutoLogon[Windows Sysinternals Autologon: User Desktop Session 1]
        Runner[GitHub Actions Self-Hosted Runner (run.cmd in User Session)]
        GPU[Physical / Virtual GPU: DirectX 9/11 3D Acceleration]
        GameClient[Original ROS Game: RosInfinity/Game/ros.exe]
        FFmpeg[FFmpeg Desktop Video Grab & GDI Screenshot Capture]
    end

    GH -->|1. Dispatches Job via Outbound HTTPS 443| Runner
    AutoLogon -->|Ensures Active Desktop| Runner
    Runner -->|2. Launches ros.exe with 3D Context| GameClient
    GPU -->|Renders 3D NeoX Engine| GameClient
    FFmpeg -->|3. Captures 1080p PNG & MP4| GameClient
    Runner -->|4. Uploads Real Evidence| Artifacts
```

### Key Technical Mechanisms
1. **Interactive Session (Session 1 vs. Session 0):**
   - GitHub Actions runners installed as a background Windows Service default to Session 0 (no GUI display).
   - **Configuration:** Use Windows Sysinternals `Autologon` to automatically log into the user desktop account on VM boot.
   - Place the runner start script (`run.cmd`) in the user's Windows Startup folder (`shell:startup`) or a Scheduled Task triggered *"At log on of any user"*.
   - The runner runs inside the active desktop session, granting `ros.exe` full access to the Direct3D display driver.
2. **Hardware 3D Acceleration:**
   - NVIDIA GRID / Studio drivers are installed on the VM.
   - DirectX 9.0c / DirectX 11 runtime and Direct3D acceleration are fully enabled.
3. **Screen & Video Capture Support:**
   - In an active interactive user session, `[System.Drawing.Graphics]::CopyFromScreen` captures crisp PNG images of the game window and desktop.
   - `ffmpeg -f gdigrab -framerate 20 -i desktop -pix_fmt yuv420p` records real-time MP4 video with genuine engine motion.

---

## 4. Secure Game File Transfer (Zero Public Git Leakage)

Per strict preservation and copyright rules:
> *“Do not upload proprietary ROS game files into the Git repository.”*

### Approved Transfer Protocol
The 6.0 GB Rules of Survival client (`RosInfinity-client-1.9.0.zip`) will be installed on the cloud VM using one of two secure paths:

#### Option 1: Direct Authorized CDN Download (Fastest & Simplest)
During the one-time VM setup, execute the following PowerShell command directly inside the cloud VM:
```powershell
$downloadUrl = "https://dl.rosinfinity.com/RosInfinity-client-1.9.0.zip"
$destZip = "C:\Temp\RosInfinity.zip"
$targetDir = "C:\Games\RulesOfSurvival"

Invoke-WebRequest -Uri $downloadUrl -OutFile $destZip
Expand-Archive -Path $destZip -DestinationPath $targetDir -Force
Remove-Item $destZip -Force

# Set persistent system environment variable
[System.Environment]::SetEnvironmentVariable("ROS_GAME_DIR", "$targetDir\RosInfinity\Game", "Machine")
```
- **Time:** ~90 seconds over the cloud provider's gigabit connection.
- **Security:** Completely isolated to the private VM; 0 bytes of proprietary assets touch GitHub.

#### Option 2: Pre-Signed Private Cloud Storage Link
If a custom archive is preferred:
- Upload the archive to a private, non-public AWS S3 bucket or Google Cloud Storage bucket.
- Generate a time-limited pre-signed URL (expires in 60 minutes).
- Download and extract on the test VM using the temporary link.

---

## 5. Workflow Integration & End-to-End Test Execution

### Step-by-Step Execution Lifecycle
1. **Start VM:**
   - Start the cloud GPU instance (via provider web console or mobile app).
   - The VM boots, automatically logs into the desktop (Session 1), and starts the GitHub Actions runner listener in the background.
2. **Trigger Test from Mobile:**
   - Open GitHub in your mobile browser or the GitHub Mobile app.
   - Go to: **[Actions > Test 1 - Original ROS Real Launch](https://github.com/redsignalofficial/ROS-Launcher/actions/workflows/ros-real-launch-test.yml)**.
   - Select runner: `self-hosted-gpu` (Default).
   - Tap **Run workflow**.
3. **Automated Test Execution on VM:**
   - The VM picks up the job.
   - It audits OS, RAM, and GPU (`Get-CimInstance Win32_VideoController`).
   - It hashes `ros.exe`, `assets.npk`, and `script.npk` (SHA-256 before launch).
   - It launches `ros.exe` with working directory set to `C:\Games\RulesOfSurvival\RosInfinity\Game`.
   - FFmpeg begins screen recording.
   - The runner polls window handles, captures screenshots of startup, loading screen, and connection dialog state.
   - The runner closes `ros.exe` after the 35-second test window.
   - It re-hashes game files to verify immutability (0 modifications).
   - It compiles `Real-Launch-Report.json` and updates `Documentation/ROS-Real-Launch-Test-Report.md`.
4. **Download Evidence on Mobile:**
   - Within 2 minutes, GitHub Actions receives the uploaded artifacts:
     - `ROS-Real-Launch-Screenshots` (Real in-game PNGs)
     - `ROS-Real-Launch-Videos` (Real MP4 video)
     - `ROS-Real-Launch-Logs` (Hardware and process logs)
     - `ROS-Real-Launch-Report` (Markdown summary)
   - You can download and view the authentic evidence directly on your phone.
5. **Stop VM:**
   - Stop or deallocate the VM to stop all billing until the next test run.

---

## 6. Recommended Action & Decision Options

Please review the following options and reply with your preferred approach:

- **Option A (Recommended — $0 Cost):** Use a Google Cloud Platform (GCP) or Azure Free Account with free credits ($200–$300 credit). Total expense: **$0.00**.
- **Option B (Pay-As-You-Go — ~$0.35/hour):** Use an AWS EC2 `g4dn.xlarge` Spot Instance. Pay only for the minutes used (roughly 20–35 cents per test session).
- **Option C (Dedicated Mobile Gaming Cloud PC — $29.99/mo):** Use Shadow PC, which gives you an always-ready Windows 11 gaming desktop with a dedicated iOS/Android app to interact with the game visually from your phone.
- **Option D (Hold / Alternative Strategy):** Request further adjustments to the plan before taking any action.

> [!NOTE]
> No paid resources will be created or billed without your explicit command. We remain strictly halted on Test 1 awaiting your approval.
