# ROS REVIVAL — ZERO-BUDGET WINDOWS GPU TESTING STRATEGY

**Project:** ROS REVIVAL (Rules of Survival Preservation Project)  
**Objective:** Execute Real Rules of Survival (`ros.exe`) with 3D DirectX GPU Acceleration Under a Strict **$0.00 Out-of-Pocket** Policy  
**Document Version:** 2.0.0  
**Status:** REVISED FOR ZERO BUDGET — AWAITING COMMUNITY / TESTER ONBOARDING  

---

## 1. Executive Summary & Policy Shift

### Background & Constraint
- The developer uses a mobile device and does not have a personal Windows PC.
- Attempting commercial cloud free trials (Google Cloud Platform) failed because virtual/prepaid debit cards are declined by payment gateway fraud/AVS verification.
- **Strict Project Directive:**
  - **Zero Paid Cloud Services:** No GCP, AWS, Shadow PC, AirGPU, or paid hosting of any kind.
  - **Zero Card Requirements:** No services requiring credit or debit card verification.
  - **Zero Policy Bypasses:** No circumventing provider terms, payment checks, or free-tier quotas.
  - **Absolute Reality:** Never simulate 3D game execution on headless Linux or report fake success. `TEST 1` remains **`TEST_BLOCKED`** until verified on real Windows GPU hardware.

### The Zero-Budget Solution
Pivot to an **Authorized Volunteer Community Testing Model** and **Zero-Card Free Tier Programs**. A community contributor with an existing Windows gaming PC connects as a temporary, secured GitHub Actions self-hosted runner, or runs the automated verification script locally and submits unmanipulated visual evidence.

---

## 2. Comparison of Legitimate Zero-Cost Testing Options

| Testing Method | Hardware Environment | Windows Desktop & DirectX 3D? | Credit Card Required? | Out-of-Pocket Cost | Feasibility & Recommendation |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **1. Volunteer Community Contributor as Self-Hosted Runner** | Physical Windows PC (NVIDIA / AMD / Intel GPU) | **YES** (Native Session 1, True Direct3D) | **NO** | **$0.00** | ⭐ **RECOMMENDED PRIMARY**: 100% automated via GitHub Actions; authentic gaming hardware. |
| **2. Manual Community Beta Testing Protocol** | Physical Windows PC (NVIDIA / AMD GPU) | **YES** (Native Desktop) | **NO** | **$0.00** | ⭐ **RECOMMENDED SECONDARY**: Volunteer downloads launcher ZIP, runs test, uploads raw logs & PNG screenshots. |
| **3. Azure for Students (GitHub Student Pack)** | Cloud GPU VM (`NVadsA10v5`) | **YES** (Windows Server + GPU) | **NO** (Requires student `.edu` email) | **$0.00** ($100 credit) | **VIABLE IF ELIGIBLE**: Valid only if the developer or a collaborator has a verified student email. |
| **4. Google Colab / Kaggle / Paperspace** | Cloud Linux Container (Ubuntu) | ❌ **NO** (Headless Linux only, no Windows DirectX) | NO | $0.00 | ⛔ **DISQUALIFIED**: Headless Linux cannot run Windows DirectX 9/11 NeoX game client. |
| **5. Oracle Cloud Always Free** | Cloud VM (ARM / AMD Micro) | ❌ **NO** (No free GPUs; CPU only) | YES | $0.00 | ⛔ **DISQUALIFIED**: Free tier has 0 GPUs; GPU shapes require paid upgrade and card. |

---

## 3. Disqualification Analysis of Headless Linux "Free GPUs"

It is tempting to look at free cloud GPU environments like Google Colab, Kaggle, or HuggingFace, but they are technically incapable of executing the original Rules of Survival PC game:

1. **Operating System Incompatibility:** They provide headless Ubuntu Linux Docker containers. Rules of Survival (`ros.exe`) is a Win32/x64 PE binary compiled against Windows DirectX (Direct3D 9/11).
2. **Missing Desktop Session:** Colab and Kaggle have no graphical desktop session (no X11/Wayland display server, no Windows Session 1). `ros.exe` requires a window handle to initialize the engine viewport.
3. **WINE / Proton Incompatibility:** The original client relies on NetEase anti-cheat drivers (`NEACSDK.cfg`, `h45na.prot`) and Direct3D middleware that fail or crash under translation layers without modifying game files.
4. **Preservation Policy Violation:** Running through experimental emulation layers violates our rule: *"Do not claim game functionality works until verified on native Windows."*

---

## 4. Volunteer Community Runner Architecture (Primary Zero-Cost Solution)

The most legitimate, authentic, and zero-cost approach is utilizing a Windows gaming PC owned by a trusted community volunteer, collaborator, or tester:

```mermaid
flowchart TD
    subgraph Mobile Developer
        Dev[Mobile Browser / GitHub App: Triggers ros-real-launch-test.yml]
    end

    subgraph GitHub Cloud Repository (redsignalofficial/ROS-Launcher)
        Repo[Actions Queue: Job waiting for 'self-hosted, Windows, GPU']
        Artifacts[GitHub Actions Artifacts: Real Screenshots & Video]
    end

    subgraph Volunteer Windows Gaming PC
        Runner[Official GitHub Actions Runner (run.cmd in User Session)]
        GPU[Hardware GPU: NVIDIA GeForce / AMD Radeon]
        Game[Authorized Original Game: RosInfinity/Game/ros.exe]
        Capture[run-ros-real-launch.ps1: Hashing, FFmpeg MP4, PNGs]
    end

    Dev -->|1. Tap 'Run workflow' on Phone| Repo
    Runner -->|2. Outbound HTTPS 443 Polls Job| Repo
    Runner -->|3. Executes ros.exe with Physical GPU Context| Game
    GPU -->|Renders 3D NeoX Graphics| Game
    Capture -->|4. Verifies Hashes & Captures Visuals| Game
    Capture -->|5. Uploads Authentic Evidence| Artifacts
```

### Why This Works Perfectly:
1. **Zero Financial Cost:** Uses existing consumer hardware already owned by a community member.
2. **Zero Card Requirements:** No accounts to create on AWS/GCP, no risk of card declines or recurring billing.
3. **Authentic Gaming Environment:** Tests `ros.exe` on real consumer gaming GPUs (e.g., GTX 1060, RTX 2060, RTX 3060, RX 580) where Rules of Survival was originally meant to run.
4. **Outbound Security:** The GitHub Actions runner software only makes **outbound HTTPS connections** to `github.com`. It requires no open ports, no port forwarding, and no router configuration on the volunteer's network.

---

## 5. Step-by-Step Volunteer Runner Setup Guide

If you or a community volunteer has access to a Windows PC with a dedicated GPU, here is the exact 5-minute setup:

### Step 1: Generate Runner Token on GitHub (Mobile or PC)
1. Go to repository settings: **[Settings > Actions > Runners > New self-hosted runner](https://github.com/redsignalofficial/ROS-Launcher/settings/actions/runners/new)**.
2. Select **Windows** and **x64**.
3. Copy the registration token shown on the page.

### Step 2: Download & Configure on Volunteer Windows PC
In an Administrator PowerShell window:
```powershell
# 1. Create runner directory
mkdir C:\actions-runner; cd C:\actions-runner

# 2. Download official GitHub Actions runner
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.321.0/actions-runner-win-x64-2.321.0.zip -OutFile runner.zip
Expand-Archive -Path runner.zip -DestinationPath . -Force
Remove-Item runner.zip

# 3. Configure runner with required labels
./config.cmd --url https://github.com/redsignalofficial/ROS-Launcher --token <PASTE_REGISTRATION_TOKEN> --labels "self-hosted,Windows,ROS-Test,Interactive,GPU" --unattended

# 4. Set game path environment variable (pointing to volunteer's Game folder)
[System.Environment]::SetEnvironmentVariable("ROS_GAME_DIR", "C:\Games\RulesOfSurvival\Game", "Machine")
```

### Step 3: Start Runner in Interactive Desktop
In the volunteer's standard desktop session:
```powershell
./run.cmd
```
*The runner connects and displays `Listening for Jobs`.*

### Step 4: Execute Test 1 from Your Phone
1. Open GitHub on your mobile browser: **[Actions > Test 1 - Original ROS Real Launch](https://github.com/redsignalofficial/ROS-Launcher/actions/workflows/ros-real-launch-test.yml)**.
2. Tap **Run workflow**.
3. The volunteer's PC automatically executes `run-ros-real-launch.ps1`, starts `ros.exe`, takes genuine in-game screenshots and MP4 video, re-verifies file hashes, and uploads the artifacts.
4. You download the genuine evidence directly to your phone.
5. The volunteer presses `Ctrl+C` in the terminal to close the runner.

---

## 6. Offline Launcher Development While Awaiting Hardware

While community testing hardware is organized, development does **not** stop. We will continue advancing the project 100% offline and cost-free:

1. **Lobby & UI Maturation:** Enhance the Custom Lobby interface, room configurations, and map selections.
2. **Offline Player Profiles:** Add local stats, loadout customization, and local match preset saving.
3. **Hash Manifest Automation:** Build automated SHA-256 integrity manifest generation tools for game directory verification.
4. **Unit Test Coverage:** Continue maintaining 100% passing automated test coverage across Linux and Windows cloud CI.

---

## 7. Status & Non-Negotiable Boundaries

- **`TEST 1` Remains:** **`TEST_BLOCKED`** (truthfully reflecting that `ros.exe` has not executed on a physical GPU yet).
- **Cloud Spending:** **$0.00**. No paid cloud services will ever be activated.
- **Game Files:** Proprietary game files remain strictly outside GitHub.

---

### Recommended Next Action
Adopt **Option 1 (Volunteer Community Contributor as a Self-Hosted Runner)** as our official strategy for unblocking Test 1. 

**STOPPED. Awaiting your confirmation.**
