# SimLock

A kiosk-style lock screen application for Windows designed for golf simulator environments. SimLock ensures new users watch a tutorial video before accessing the system, while returning users can unlock with a PIN code.

## Overview

SimLock is designed for businesses running golf simulators (such as GSPro) where you want to:
- Ensure first-time users watch a mandatory tutorial video
- Allow returning users to quickly unlock with a PIN
- Prevent users from bypassing the lock screen

When the monitored application (e.g., `gspro.exe`) launches, SimLock automatically displays a full-screen lock overlay with two options:
1. **Returning Golfer** - Enter a 4-digit PIN to unlock
2. **First Time? Watch Tutorial** - Watch the tutorial video to completion to unlock

## Features

- **Full-screen kiosk mode** - Blocks Alt+Tab, Windows key, and other system shortcuts
- **Touch-friendly interface** - Large buttons designed for touchscreen displays
- **Automatic process monitoring** - Launches lock screen when target application starts
- **PIN authentication** - 4-digit code for returning users
- **Mandatory video playback** - Tutorial video must play to completion (no skip/fast-forward)
- **Time remaining display** - Shows countdown during video playback
- **Auto-unlock** - System unlocks automatically when video completes
- **System tray monitor** - Runs quietly in background watching for target process
- **Admin panel** - Configure all settings through a password-protected interface
- **YouTube video download** - Download tutorial videos directly from YouTube URLs
- **Custom branding** - Add your own logo to all screens
- **Configurable messages** - Customize splash screen text and custom messages

## System Requirements

- Windows 10 or Windows 11 (64-bit)
- .NET 8 Desktop Runtime (installer will download automatically if not present)
- Minimum 4GB RAM
- Internet connection (for initial setup and video download)

## Installation

1. Download `SimLock_Setup.exe` from the [Releases](../../releases) page
2. Run the installer as Administrator
3. If .NET 8 is not installed, click OK to download and install it automatically
4. Follow the installation wizard
5. After installation, the Admin Panel will open automatically

### Installer Options

- **Create desktop shortcut** - Checked by default
- **Start SimLock Monitor with Windows** - Recommended for automatic startup

## Initial Setup

### 1. Log into Admin Panel

- Launch **SimLock Admin** from the Start Menu or Desktop
- Default password: `admin123`
- **Important:** Change the admin password after first login

### 2. Configure Settings

#### Security Settings
- **Admin Password** - Change from default immediately
- **Unlock PIN** - Set the 4-digit code for returning users (default: 1234)

#### Video Settings
- **YouTube URL** - Paste a YouTube video URL
- **Download Video** - Downloads the video for offline playback
- Videos are saved to `C:\ProgramData\SimLock\Videos\`

#### Branding
- **Logo** - Upload your company logo (PNG recommended, 256x256px)
- Logo appears on all screens

#### Splash Screen
- **Title** - Main heading on splash screen
- **Subtitle** - Secondary text below title

#### Screen Customization
- **PIN Screen Title** - Heading for the PIN entry screen
- **Custom Message** - Optional message shown on all screens

#### Process Monitor
- **Monitored Process** - The application that triggers the lock screen
- Click **Select Process** to choose from running applications
- Default: `gspro` (for GSPro golf simulator)

### 3. Save Settings

Click **Save Settings** to apply all changes. The monitor will automatically reload the configuration.

## How It Works

### Automatic Lock Screen

1. **SimLock Monitor** runs in the system tray (shield icon)
2. When the monitored process (e.g., GSPro) starts, the lock screen appears
3. User must either:
   - Enter the correct PIN (returning user), OR
   - Watch the entire tutorial video (new user)
4. After successful authentication, the lock screen closes
5. Monitor resets and waits for the next process launch

### For Returning Users

1. Click **"Returning Golfer"**
2. Enter the 4-digit PIN using the on-screen numpad or keyboard
3. System unlocks immediately upon correct PIN entry
4. Incorrect PIN shows error message and clears input

### For New Users

1. Click **"First Time? Watch Tutorial"**
2. Video plays automatically at full volume
3. Time remaining is displayed at the bottom
4. User can click **"Back to Main Menu"** to exit (video stops)
5. When video completes, system unlocks automatically

## File Locations

| Item | Location |
|------|----------|
| Application | `C:\Program Files\SimLock\` |
| Configuration | `C:\ProgramData\SimLock\config.json` |
| Downloaded Videos | `C:\ProgramData\SimLock\Videos\` |
| Uploaded Assets | `C:\ProgramData\SimLock\Assets\` |
| yt-dlp | `C:\ProgramData\SimLock\yt-dlp.exe` |
| ffmpeg | `C:\ProgramData\SimLock\ffmpeg.exe` |

## System Tray Menu

Right-click the shield icon in the system tray:
- **Open Admin Panel** - Launch the configuration interface
- **Launch Lock Screen** - Manually trigger the lock screen (for testing)
- **Reload Config** - Reload settings without restarting
- **Exit** - Close the monitor (lock screen will not trigger)

## Troubleshooting

### Lock screen doesn't appear when application starts
- Ensure SimLock Monitor is running (check system tray for shield icon)
- Verify the correct process name is configured in Admin Panel
- Try reloading config from the tray menu

### Video won't play
- Ensure video was downloaded successfully
- Check that the video file exists in `C:\ProgramData\SimLock\Videos\`
- Try re-downloading the video from Admin Panel

### Can't download YouTube video
- Ensure you have an internet connection
- yt-dlp and ffmpeg download automatically on first use (~90MB total)
- Some videos may be region-restricted or private

### PIN not working
- Verify the correct PIN in Admin Panel
- PIN is exactly 4 digits
- Check for num lock if using keyboard numpad

### Admin Panel won't open
- Default password is `admin123`
- If password was changed and forgotten, delete `C:\ProgramData\SimLock\config.json` to reset

## Uninstallation

1. Run the uninstaller from Start Menu or Control Panel
2. All SimLock processes will be stopped automatically
3. Application files and settings in ProgramData will be removed

## Security Notes

- The lock screen blocks most system shortcuts but is not a security product
- Determined users could potentially bypass it (e.g., Task Manager if not disabled)
- For higher security, consider Windows Kiosk Mode or third-party solutions
- Store the admin password securely - it's stored in plain text in config.json

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 8 SDK
- Inno Setup 6 (for creating installer)

### Build Steps
```bash
# Clone repository
git clone https://github.com/yourusername/SimLock.git
cd SimLock

# Build all projects
dotnet build SimLock.sln -c Release

# Publish for distribution
dotnet publish src/SimLock.Locker -c Release -o publish
dotnet publish src/SimLock.Admin -c Release -o publish
dotnet publish src/SimLock.Monitor -c Release -o publish
dotnet publish src/SimLock.Launcher -c Release -r win-x64 --self-contained -o publish

# Create installer (requires Inno Setup)
iscc installer/SimLock_Setup.iss
```

## License

Copyright (c) 2024 SimWoods Golf. All rights reserved.

## Support

For issues and feature requests, please use the [GitHub Issues](../../issues) page.
