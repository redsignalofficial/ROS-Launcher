# ROS REVIVAL — CLOUD WINDOWS GPU TESTING PLAN

**Project:** ROS REVIVAL (Rules of Survival Preservation Project)  
**Objective:** Execute Real Rules of Survival (`ros.exe`) on a Cloud Windows GPU Environment without Physical PC Hardware  
**Document Version:** 1.1.0  
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
> No cloud instance will be purchased, rented, deployed, or activated without your explicit review and approval. Multiple free-tier and low-cost options are detailed below.

---

## 2. Cloud Provider Comparison & Cost Analysis

| Provider / Service | GPU Hardware | OS & Desktop Session | Free Credits / Trial | Hourly Cost | Estimated Cost Per Test Run (30 min) | Monthly 24/7 (Reference) |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Google Cloud Platform (GCP)** *(Recommended for $0)* | NVIDIA Tesla T4 (16GB VRAM) | Windows Server 2022 + NVIDIA GRID (Interactive Session 1) | **$300 Free Credit** (90 days) | ~$0.71 / hr | **$0.00** *(Deducted from free credit)* | ~$510 / mo |
| **Microsoft Azure** *(Recommended for $0)* | NVIDIA A10 (`NVadsA10v5`) or Tesla M60 | Windows 10/11 Multi-Session or Server 2022 | **$200 Free Credit** (30 days) | ~$0.75 / hr | **$0.00** *(Deducted from free credit)* | ~$540 / mo |
| **AWS EC2 (Spot Instance)** *(Lowest Pay-As-You-Go)* | NVIDIA Tesla T4 (`g4dn.xlarge`, 4 vCPU, 16GB RAM) | Windows Server 2022 with Desktop Experience | AWS Free Tier (Standard credits) | **~$0.35 – $0.45 / hr** | **~$0.18 – $0.23** | ~$250 / mo |
| **AirGPU** *(Gaming-Optimized Pay-As-You-Go)* | NVIDIA Tesla T4 / L4 (GTX 1080 / RTX class) | Windows 10/11 Desktop + Parsec / RDP | Credit-based ($10 min deposit) | ~$0.65 – $0.90 / hr | **~$0.33 – $0.45** | Pay-as-you-go + $3.50/mo storage |
| **Shadow PC** *(Dedicated Monthly Cloud PC)* | NVIDIA RTX 2000 Ada / RTX 3070 equivalent | Full Windows 11 Home/Pro Desktop (Mobile iOS/Android App) | None | N/A (Flat subscription) | Included in flat fee | **$29.99 – $34.99 / mo** |

---

## 3. Deep Dive: GCP Windows GPU Configuration & Quotas

If **Google Cloud Platform (GCP)** is selected to leverage the $300 free trial credit:

### Itemized GCP Hourly Cost Breakdown
- **Compute Instance (`n1-standard-4`):** 4 vCPUs, 15 GB RAM = **$0.190 / hr**
- **GPU Accelerator:** 1x NVIDIA Tesla T4 = **$0.350 / hr**
- **Windows Server License:** 4 vCPUs × $0.040 = **$0.160 / hr**
- **Storage:** 50 GB Balanced Persistent Disk = **$0.007 / hr** (~$5.00/mo)
- **Total Combined Hourly Cost:** **~$0.707 / hour**
- **Test Session (30 minutes):** **~$0.35** (Fully covered by the $300 credit; **$0.00 out of pocket**).

### Quota Prerequisite & Approval Reality
- New Google Cloud accounts default to a **GPU quota of 0** (`GPUS_ALL_REGIONS = 0` / `NVIDIA_T4_GPUS = 0`).
- To provision an NVIDIA T4, an account owner must submit a quota increase request in the GCP Console (**IAM & Admin > Quotas > Compute Engine API > GPUs (all regions) > Request Increase to 1**).
- Approval typically takes between 15 minutes to 24 hours. Google sometimes requires upgrading from the free trial billing tier (with a verified card) before granting GPU quota.
- **Alternative if GCP quota is delayed:** Azure ($200 free credit) or an AWS EC2 `g4dn.xlarge` Spot Instance (~$0.35/hr, no monthly commitment).

---

## 4. Interactive Remote Desktop Architecture

DirectX games like Rules of Survival (`ros.exe`) require an active user desktop (Session ID ≥ 1) and fail with `D3DERR_NOTAVAILABLE` when launched in a background Windows Service session (Session 0).

```mermaid
flowchart TD
    subgraph Mobile Device
        Mobile[Developer Mobile Phone: Chrome Remote Desktop App / Browser]
    end

    subgraph GitHub Cloud
        GH[GitHub Actions Workflow Dispatch]
        Artifacts[GitHub Actions Artifacts: Screenshots & Videos]
    end

    subgraph Cloud Windows GPU VM
        AutoLogon[Windows Sysinternals Autologon: Session 1]
        CRD[Chrome Remote Desktop Host Service]
        Runner[GitHub Actions Self-Hosted Runner (run.cmd)]
        DirectX[NVIDIA GPU Driver + DirectX 9/11 Acceleration]
        Game[ros.exe + assets.npk + script.npk]
        Capture[FFmpeg MP4 Grab & GDI Screenshot Capture]
    end

    Mobile -->|View Live Game Screen on Phone| CRD
    CRD --> AutoLogon
    GH -->|Trigger Test 1 Outbound 443| Runner
    AutoLogon --> Runner
    Runner -->|Launch Game with 3D Context| Game
    DirectX --> Game
    Game --> Capture
    Capture -->|Upload Real Evidence| Artifacts
```

### Safe Interactive Setup: Chrome Remote Desktop (CRD)
1. **Firewall Safe:** Connects entirely outbound over WebRTC via Google servers.
2. **No Port Openings:** Inbound Windows RDP port (3389) remains closed, protecting the VM from unauthorized network scans.
3. **Mobile Accessible:** You can open the Chrome Remote Desktop app on your phone, see the genuine Windows desktop, and view the game window live on your screen.
4. **Sysinternals Autologon:** Ensures the VM automatically logs into the desktop on boot so the GitHub Actions runner starts in Session 1, even if you are not actively connected via remote desktop.

---

## 5. Secure Game File Transfer (Zero Public Git Leakage)

Per strict preservation and copyright rules:
> *“Do not upload proprietary ROS game files into the Git repository.”*

### Approved Transfer Protocol
The 6.0 GB Rules of Survival client (`RosInfinity-client-1.9.0.zip`) will be installed on the cloud VM using direct download on the VM:
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
- Completely isolated to the private VM; zero bytes of proprietary game files touch GitHub.

---

## 6. Workflow Integration & Test 1 Artifact Return

1. **Trigger:** You tap **Run workflow** on `ros-real-launch-test.yml` from GitHub on your phone.
2. **Execute:** The self-hosted runner executes `run-ros-real-launch.ps1`.
3. **Capture:**
   - Hashes `ros.exe`, `assets.npk`, `script.npk` before launch.
   - Launches `ros.exe` in `C:\Games\RulesOfSurvival\RosInfinity\Game`.
   - Records desktop video via FFmpeg and captures in-game screenshots.
   - Observes startup screen and connection timeout state.
   - Re-hashes files to verify 100% immutability.
4. **Return:**
   - Uploads `ROS-Real-Launch-Screenshots`, `ROS-Real-Launch-Videos`, `ROS-Real-Launch-Logs`, and `ROS-Real-Launch-Report` back to the GitHub workflow run.
   - You download the authentic visual evidence directly to your mobile device.

---

## 7. Next Steps & Decision Options

Please select one of the following paths:

- **Option A (GCP $0 Free Trial):** Set up a free Google Cloud account, request 1x T4 GPU quota, and provision a temporary Windows Server VM.
- **Option B (AWS Spot Pay-As-You-Go):** Spin up an AWS `g4dn.xlarge` Spot Instance on demand (~35¢/hr, under 25¢ per test session).
- **Option C (Shadow PC Dedicated Gaming VM):** Use Shadow PC ($29.99/mo) with full mobile app interaction.
- **Option D (Hold / Alternative Strategy):** Defer Test 1 execution and continue refining the offline launcher features.

> [!NOTE]
> No paid cloud instances have been created or billed. We remain stopped awaiting your direction.
