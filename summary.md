# SimLock - Golf Simulator Kiosk Lock Screen

## Overview

SimLock is a professional .NET 8 WPF kiosk lock screen application designed for golf simulators. It provides a secure, customizable splash/lock screen that automatically activates when a monitored application (like GSPRO Launcher) starts, requiring user interaction before allowing access to the simulator.

**Key Use Case:** Golf simulation facilities use SimLock to display branding, tutorials, and information to customers before they begin their session, while also providing PIN-based access control for returning golfers.

---

## Project Structure

```
SimLock/
├── src/
│   ├── SimLock.Admin/          # Admin configuration panel
│   ├── SimLock.Locker/         # Main lock screen application
│   ├── SimLock.Monitor/        # Background process monitor
│   ├── SimLock.Launcher/       # Manual launcher for testing
│   └── SimLock.Common/         # Shared library
├── activation-server/          # Python Flask license server
├── installer/                  # Inno Setup installer files
├── Assets/                     # Icons and images
├── snaps/                      # Screenshots and documentation
└── config.json                 # Runtime configuration
```

---

## Components

### 1. SimLock.Locker (`SimLock.exe`)
The main lock screen application featuring:
- **Splash Screen** - Customizable welcome screen with title, subtitle, logo, and background image
- **Main Menu** - Configurable buttons for user navigation
- **PIN Entry** - 4-digit keypad for returning golfers
- **Video Player** - Built-in player for tutorial videos with countdown timer
- **Theme Support** - Full color and font customization
- **Custom Actions** - Buttons that can open programs, websites, PDFs, pictures, or videos

### 2. SimLock.Admin (`SimLock.Admin.exe`)
Configuration panel with 6 tabs:

| Tab | Features |
|-----|----------|
| **General** | Unlock code, admin password, monitored process, GSPRO quick-select |
| **Appearance** | Splash title/subtitle, logo, background image, opacity settings |
| **Buttons** | Standard buttons toggle, 2 custom buttons with 5 action types |
| **Video** | YouTube URL input, local video path, download with progress |
| **Theme** | 7 color pickers, font selector (12 fonts available) |
| **Screen Text** | PIN screen title, custom footer message |

**Footer:** License activation with email-based licensing system

### 3. SimLock.Monitor (`SimLock.Monitor.exe`)
Background Windows service that:
- Watches for the configured process to start
- Automatically launches the lock screen when detected
- Runs minimized in system tray
- Can be set to start with Windows

### 4. SimLock.Launcher (`SimLock.Launcher.exe`)
Simple launcher for testing the lock screen without process monitoring.

### 5. SimLock.Common (Shared Library)
Contains:
- `AppConfig.cs` - Configuration management with JSON persistence
- `ThemeManager.cs` - Dynamic theme/color handling
- `MachineIdentifier.cs` - Hardware fingerprinting for licensing
- `ActivationService.cs` - HTTP client for activation server API

### 6. Activation Server
Flask-based Python server for license management:

| Property | Value |
|----------|-------|
| **Server** | 192.168.88.197:8443 |
| **Admin UI** | https://activation.neutrocorp.com:8443/ |
| **Credentials** | admin / SimLock2024! |
| **Database** | SQLite (activations.db) |

**API Endpoints:**
- `POST /api/check-email` - Check email for licenses and auto-activate
- `POST /api/activate` - Activate license on machine
- `POST /api/deactivate` - Deactivate license from machine
- `POST /api/check` - Check activation status

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `src/SimLock.Admin/AdminWindow.xaml` | Admin panel UI (XAML layout) |
| `src/SimLock.Admin/AdminWindow.xaml.cs` | Admin panel logic (800+ lines) |
| `src/SimLock.Admin/LoginWindow.xaml` | Admin login screen |
| `src/SimLock.Admin/ProcessSelectorDialog.xaml` | Running process picker |
| `src/SimLock.Locker/MainWindow.xaml` | Lock screen UI |
| `src/SimLock.Locker/MainWindow.xaml.cs` | Lock screen logic |
| `src/SimLock.Common/AppConfig.cs` | All 30+ configuration properties |
| `src/SimLock.Common/ActivationService.cs` | License server API client |
| `activation-server/app.py` | Flask license server |
| `installer/SimLock_Setup.iss` | Inno Setup installer script |
| `snaps/SimLock_Admin_Guide.html` | User documentation with screenshots |
| `snaps/SimLock_Video_Script.md` | Video production storyboard |

---

## Configuration Options

### Security
| Property | Description | Default |
|----------|-------------|---------|
| `UnlockCode` | 4-digit PIN code | 1234 |
| `AdminPassword` | Admin panel password | admin123 |

### Video
| Property | Description |
|----------|-------------|
| `VideoUrl` | YouTube URL for tutorial |
| `LocalVideoPath` | Path to downloaded/local MP4 file |

### Splash Screen
| Property | Description |
|----------|-------------|
| `SplashTitle` | Main heading text |
| `SplashSubtitle` | Secondary text |
| `SplashImagePath` | Logo image path |
| `SplashBackgroundImagePath` | Custom background image |
| `UseSplashBackgroundImage` | Enable custom background |
| `SplashTextBoxOpacity` | Text overlay opacity (0.5-1.0) |

### Buttons
| Property | Description |
|----------|-------------|
| `ShowReturningGolferButton` | Show PIN entry button |
| `ShowTutorialButton` | Show tutorial video button |
| `ShowCustomButton1/2` | Enable custom buttons |
| `CustomButton1Label` | Button text |
| `CustomButton1ActionType` | Action type (see below) |
| `CustomButton1Target` | File path or URL |

**Action Types:**
- `RunExecutable` - Start a program (displayed as "Start Program")
- `OpenUrl` - Open website (displayed as "Open Website")
- `PlayLocalVideo` - Play video in built-in player
- `OpenPdf` - Open PDF document
- `OpenPicture` - Open image file

### Theme Colors
| Property | Used For |
|----------|----------|
| `ThemePrimaryColor` | Main buttons & headers |
| `ThemeSecondaryColor` | Button hover states |
| `ThemeAccentColor` | Highlights & links |
| `ThemeBackgroundColor` | Main screen background |
| `ThemeSurfaceColor` | Cards & panels |
| `ThemeTextPrimaryColor` | Main text |
| `ThemeTextSecondaryColor` | Subtle text |
| `ThemeFontFamily` | Application font |

### Activation
| Property | Description |
|----------|-------------|
| `IsActivated` | Current activation status |
| `ActivationEmail` | Licensed email address |
| `LicenseKey` | License key from server |
| `MachineId` | Hardware fingerprint |
| `ActivationServerUrl` | Server URL |

---

## Building & Deployment

### Requirements
- .NET 8 SDK
- Windows 10/11 (WPF application)
- Visual Studio 2022 or VS Code
- Inno Setup (for installer)

### Build Commands
```bash
# Build all projects
dotnet build

# Publish for release
dotnet publish -c Release -o ./publish

# Create installer (run on Windows)
# Open installer/SimLock_Setup.iss in Inno Setup Compiler
```

### Deployment Workflow

1. **Build on Linux:**
   ```bash
   cd /home/csolaiman/SimLock
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to Windows Share:**
   ```bash
   scp -i ~/.ssh/dms-deploy <files> csolaiman@192.168.88.197:/mnt/windev/
   ```

3. **Build Installer on Windows:**
   - Open `installer/SimLock_Setup.iss` in Inno Setup
   - Compile to create `installer_output/SimLock_Setup.exe`

4. **Create Distribution ZIP:**
   ```powershell
   Compress-Archive -Path installer_output\SimLock_Setup.exe -DestinationPath SimLock_Installer.zip
   ```

### Activation Server Management
```bash
# SSH to server
ssh -i ~/.ssh/dms-deploy csolaiman@192.168.88.197

# Start/restart server
cd /home/csolaiman/activation-server
source venv/bin/activate
pkill -f 'python app.py'
nohup python app.py > /tmp/activation.log 2>&1 &

# Or use systemd
sudo systemctl restart simlock-activation
```

---

## Technical Notes

### WinForms/WPF Namespace Resolution
The project uses `UseWindowsForms=true` for the color picker dialog, which creates namespace conflicts. Resolved using type aliases in AdminWindow.xaml.cs:

```csharp
using WinForms = System.Windows.Forms;
using WpfColor = System.Windows.Media.Color;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfMessageBox = System.Windows.MessageBox;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfButton = System.Windows.Controls.Button;
```

### Video Download
Uses yt-dlp and ffmpeg for downloading YouTube videos:
- Downloads yt-dlp.exe automatically if not present
- Downloads ffmpeg.exe automatically if not present
- Shows real-time progress using async stdout/stderr reading
- Outputs MP4 format for maximum compatibility

### App Icons
Valid .ico file required (not PNG renamed to .ico). Located at `Assets/app.ico` and `installer/app.ico`. Referenced in all .csproj files via `<ApplicationIcon>` element.

---

## Documentation

### User Guide
`snaps/SimLock_Admin_Guide.html` - Comprehensive 8-chapter HTML guide with screenshots:
1. Overview
2. Installation
3. Getting Started
4. Admin Panel Configuration
5. Lock Screen Features
6. Licensing & Activation
7. Troubleshooting
8. Support

### Screenshots (in `snaps/` folder)
**Admin Panel:**
- `Capture.PNG` - Login screen
- `Capture.2.PNG` - General tab
- `Capture3.PNG` - Appearance tab
- `Capture4.PNG` - Buttons tab
- `Capture5.PNG` - Video tab
- `Capture6.PNG` - Theme tab
- `Capture7.PNG` - Screen Text tab

**Lock Screen:**
- `splashscreen.png` - Welcome splash screen
- `mainmenu.png` - Main menu with buttons
- `pinentry.png` - PIN entry keypad
- `videoplayer.png` - Tutorial video player

### Video Production
`snaps/SimLock_Video_Script.md` - Complete storyboard and narration script for creating a tutorial video.

---

## Version History

### v2.0.2 (Current)
- Added "Open PDF Document" and "Open Picture" action types
- Updated dropdown labels: "Start Program", "Open Website"
- Fixed video download hanging (async stdout/stderr)
- Fixed scrollbar overlap in admin panel
- Fixed activation server reactivation bug
- Added app icons to all executables
- Created Admin Guide and Video Script documentation

### v2.0.1
- Fixed WinForms/WPF namespace conflicts (CS0104 errors)
- Rewrote AdminWindow.xaml.cs with using aliases
- Fixed ambiguous type references across all files

### v2.0
- Password field visibility fix
- Save button "Saved!" feedback
- Test button no longer auto-saves
- Full color picker dialogs
- 12 font options
- Clearer theme color labels
- GSPRO Launcher quick-select button
- Activation API `/api/check-email` endpoint
- Status messages in UI
- App icon support in installer

---

## Support & Contact

| Resource | Details |
|----------|---------|
| **Company** | NeutroCorp LLC |
| **Website** | https://www.neutrocorp.com |
| **Email** | support@neutrocorp.com |
| **GitHub** | https://github.com/cstk421/SimWoods-Golf |

---

## Next Context Prompt

Copy this to continue work in a new Claude session:

```
I'm continuing work on SimLock, a .NET 8 WPF kiosk lock screen for golf simulators.

Project location: /home/csolaiman/SimLock
Activation server: 192.168.88.197 (/home/csolaiman/activation-server/)
Windows share: //192.168.88.156/Users/cstk421/Downloads/Dev (mounted at /mnt/windev on activation server)

Key files:
- src/SimLock.Admin/AdminWindow.xaml(.cs) - Admin panel (uses WPF type aliases for WinForms compatibility)
- src/SimLock.Locker/MainWindow.xaml(.cs) - Lock screen
- src/SimLock.Common/AppConfig.cs - Configuration (30+ properties)
- activation-server/app.py - Flask license server
- installer/SimLock_Setup.iss - Inno Setup installer
- snaps/SimLock_Admin_Guide.html - User documentation

GitHub: https://github.com/cstk421/SimWoods-Golf
SSH: ssh -i ~/.ssh/dms-deploy csolaiman@192.168.88.197

Build: dotnet publish -c Release -o publish
Deploy: scp -i ~/.ssh/dms-deploy <file> csolaiman@192.168.88.197:/mnt/windev/<path>

Current version: v2.0.2
- All namespace conflicts resolved
- 5 custom button action types
- Video download with progress
- Full documentation complete

Please read summary.md for full project context.
```
