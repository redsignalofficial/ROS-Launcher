# ROS REVIVAL — Volunteer Runner Security & Operations Guide

## Overview

To maintain our strict **$0.00 budget** while testing the original Rules of Survival PC client (`ros.exe`) on genuine DirectX 3D graphics hardware, ROS REVIVAL utilizes **volunteer community Windows gaming PCs** as temporary GitHub Actions self-hosted runners.

Running a self-hosted runner connects an external machine to a GitHub repository to execute automated workflows. Because repository workflows can execute commands on the host machine, strict security protocols must be followed to protect both the volunteer's personal system and the repository.

---

## ⚠️ Key Security Risks

1. **Public Repository Execution Risk**:
   - In public repositories, anyone can submit a Pull Request. If workflows are triggered automatically on PRs without approval, malicious code could run on the self-hosted machine.
   - **Protection Rule**: The repository MUST require maintainer approval for all outside collaborators before running workflows (`Actions permissions -> Fork pull request workflows from outside collaborators -> Require approval`).

2. **Privilege Escalation**:
   - Workflows running as an Administrator can inspect or modify system files, install malware, or access other user data.
   - **Protection Rule**: The GitHub runner agent must NEVER run under an Administrator account.

3. **Data Remanence & Credential Theft**:
   - Personal files, browser cookies, and stored credentials on the host machine could theoretically be accessed by a compromised workflow script.
   - **Protection Rule**: Never run on a daily-driver Windows user profile without isolation. Use a dedicated clean local Windows user account.

---

## Safe Runner Setup Instructions

### Step 1: Create a Dedicated, Non-Admin Windows Account

1. Open **Settings -> Accounts -> Other Users -> Add account**.
2. Create a local user named `ros-test-runner` with a strong password.
3. Ensure this account belongs to the standard **Users** group (do NOT grant Administrator privileges).
4. Sign in to `ros-test-runner` to initialize an interactive desktop session (Session 1).
   - *DirectX 3D games require an active logged-in interactive desktop session to initialize display devices.*

### Step 2: Download and Configure the Runner

1. In the GitHub repository, navigate to:
   `Settings -> Actions -> Runners -> New runner -> Windows (x64)`.
2. Open PowerShell as `ros-test-runner` (non-elevated):
   ```powershell
   mkdir C:\actions-runner; cd C:\actions-runner
   # Download the latest runner package provided in the GitHub UI
   Invoke-WebRequest -Uri <RUNNER_DOWNLOAD_URL> -OutFile actions-runner.zip
   Expand-Archive -Path actions-runner.zip -DestinationPath .
   ```

3. **Configure as Ephemeral**:
   Always pass `--ephemeral` so that the runner executes exactly ONE test run and immediately deregisters and halts:
   ```powershell
   .\config.cmd --url https://github.com/redsignalofficial/ROS-Launcher `
                --token <REGISTRATION_TOKEN> `
                --labels self-hosted,Windows,ROS-Test,Interactive,GPU `
                --name "Volunteer-GPU-Runner" `
                --ephemeral
   ```

4. **Run Interactively (Do NOT Install as a Windows Service)**:
   - Headless background services run in **Session 0**, which lacks 3D GPU acceleration.
   - Start the runner directly in the interactive desktop console:
     ```powershell
     .\run.cmd
     ```

---

## Clean Runner Removal & De-registration

When the testing session is complete (or if the runner was interrupted), cleanly unregister and remove all runner files:

```powershell
# 1. Stop runner process (Ctrl+C if running in terminal)

# 2. De-register runner from GitHub repository
.\config.cmd remove --token <REMOVAL_TOKEN>

# 3. Delete runner workspace and cache directories
cd C:\
Remove-Item -Recurse -Force C:\actions-runner

# 4. Optional: Delete or disable the temporary local account
# (From an administrative command prompt)
net user ros-test-runner /delete
```

---

## Workflow Guardrails & Protections

To protect volunteer machines, our repository workflows (`.github/workflows/ros-real-launch-test.yml`) implement these guardrails:

- **Hard 5-minute timeout**: Every game test step enforces a strict timeout to prevent processes from hanging.
- **Process kill & cleanup trap**: Game processes and screencast monitors are aggressively terminated upon workflow completion (`Stop-Process -Name ros -Force`).
- **Strict Read-Only Game Files**: Original game files are accessed strictly read-only.
- **Zero Injected Binaries**: No non-standard DLLs, hooks, or cracked game files are executed.
- **Zero Proprietary Files in Artifacts**: Artifacts only contain visual logs, diagnostic output, and screenshots; proprietary NetEase game files are never uploaded to GitHub.
