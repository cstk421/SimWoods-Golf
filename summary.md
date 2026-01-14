# SimLock - Golf Simulator Kiosk Lock Screen

## Overview
SimLock is a .NET 8 WPF kiosk lock screen application designed for golf simulators. It provides a secure splash/lock screen that activates when a monitored application (like GSPRO Launcher) starts, requiring user interaction before allowing access.

## Components

### 1. SimLock.Locker (`SimLock.exe`)
The main lock screen application that displays:
- Splash screen with customizable title, subtitle, and background
- PIN entry screen for returning golfers
- Tutorial video playback
- Custom action buttons
- Theme-aware UI with configurable colors and fonts

### 2. SimLock.Admin (`SimLock.Admin.exe`)
Admin panel for configuring all settings:
- **General Tab**: Unlock code, admin password, monitored process
- **Appearance Tab**: Splash screen images, backgrounds, logos
- **Buttons Tab**: Configure custom action buttons
- **Video Tab**: Tutorial video URL/path, download functionality
- **Theme Tab**: Color pickers for 7 theme colors, font selection
- **Screen Text Tab**: Custom messages, PIN screen title
- **Footer**: License activation with email-based licensing

### 3. SimLock.Monitor (`SimLock.Monitor.exe`)
Background service that watches for the configured process and launches the lock screen when detected.

### 4. SimLock.Launcher (`SimLock.Launcher.exe`)
Entry point that launches the lock screen directly for testing.

### 5. SimLock.Common
Shared library containing:
- `AppConfig.cs` - Configuration management with JSON persistence
- `ThemeManager.cs` - Dynamic theme/color handling
- `MachineIdentifier.cs` - Hardware fingerprinting for licensing
- `ActivationService.cs` - Client for activation server API

### 6. Activation Server (`activation-server/`)
Flask-based Python server for license management:
- **Location**: `192.168.88.197:8443`
- **Admin UI**: `https://activation.neutrocorp.com:8443/`
- **Credentials**: `admin` / `SimLock2024!`
- **Database**: SQLite (`activations.db`)
- **API Endpoints**:
  - `POST /api/check-email` - Check email for licenses and auto-activate
  - `POST /api/activate` - Activate license on machine
  - `POST /api/deactivate` - Deactivate license from machine
  - `POST /api/check` - Check activation status

## Key Files

| File | Purpose |
|------|---------|
| `src/SimLock.Admin/AdminWindow.xaml` | Admin panel UI layout |
| `src/SimLock.Admin/AdminWindow.xaml.cs` | Admin panel logic |
| `src/SimLock.Admin/LoginWindow.xaml` | Admin login screen |
| `src/SimLock.Locker/MainWindow.xaml` | Lock screen UI |
| `src/SimLock.Common/AppConfig.cs` | All configuration properties |
| `src/SimLock.Common/ActivationService.cs` | Activation API client |
| `activation-server/app.py` | License server Flask app |
| `installer/SimLock_Setup.iss` | Inno Setup installer script |
| `config.json` | Runtime configuration |

## Configuration Options (AppConfig.cs)

### Security
- `UnlockCode` - 4-digit PIN code
- `AdminPassword` - Admin panel password

### Video
- `VideoUrl` - YouTube URL for tutorial
- `LocalVideoPath` - Path to local video file

### Splash Screen
- `SplashTitle`, `SplashSubtitle` - Text on splash screen
- `SplashImagePath` - Logo image on splash
- `SplashBackgroundImagePath` - Custom background image
- `UseSplashBackgroundImage` - Enable custom background
- `SplashTextBoxOpacity` - Opacity of text overlay (0.5-1.0)

### Buttons
- `ShowReturningGolferButton`, `ShowTutorialButton` - Standard buttons
- `ShowCustomButton1`, `ShowCustomButton2` - Custom action buttons
- `CustomButton1Label`, `CustomButton1ActionType`, `CustomButton1Target` - Button 1 config
- `CustomButton2Label`, `CustomButton2ActionType`, `CustomButton2Target` - Button 2 config
- Action types: `RunExecutable`, `OpenUrl`, `PlayLocalVideo`, `OpenPdf`, `OpenPicture`

### Theme
- `ThemePrimaryColor` - Main buttons & headers
- `ThemeSecondaryColor` - Button hover states
- `ThemeAccentColor` - Highlights & links
- `ThemeBackgroundColor` - Main screen background
- `ThemeSurfaceColor` - Cards & panels
- `ThemeTextPrimaryColor` - Main text
- `ThemeTextSecondaryColor` - Subtle text
- `ThemeFontFamily` - Application font

### Activation
- `IsActivated` - Current activation status
- `ActivationEmail` - Licensed email
- `LicenseKey` - License key from server
- `MachineId` - Hardware fingerprint
- `ActivationServerUrl` - Server URL (default: `https://activation.neutrocorp.com:8443`)

## Building

### Requirements
- .NET 8 SDK
- Windows (WPF application)
- Visual Studio 2022 or VS Code

### Build Commands
```bash
# Build all projects
dotnet build

# Publish for release
dotnet publish -c Release -o ./publish

# Run tests
dotnet test
```

### Installer
1. Place `app.ico` in `Assets/` folder
2. Build and publish the projects
3. Run Inno Setup Compiler on `installer/SimLock_Setup.iss`
4. Output: `installer_output/SimLock_Setup.exe`

## Deployment

### Windows Share
```bash
# Mount Windows share
sudo mount -t cifs //192.168.88.156/Users/cstk421/Downloads/Dev /mnt/windev -o guest

# Copy build files
cp -r publish/* /mnt/windev/SimLock/
cp installer/SimLock_Setup.iss /mnt/windev/InnoFiles/
```

### Activation Server
```bash
# On 192.168.88.197
cd /home/csolaiman/activation-server
source venv/bin/activate
python app.py

# Or use systemd service
sudo systemctl restart simlock-activation
```

## Recent Updates (v2.0)

1. **Password field visibility** - Fixed PasswordBox to show dots when typing
2. **Save button feedback** - Shows "Saved!" momentarily instead of popup
3. **Test button** - No longer auto-saves before testing
4. **Color pickers** - Full color dialog for theme customization
5. **Font options** - 12 fonts available (Segoe UI, Arial, Verdana, etc.)
6. **Theme labels** - Clear descriptions of what each color affects
7. **Common apps button** - Quick select for GSPRO Launcher
8. **Activation API fix** - Added missing `/api/check-email` endpoint
9. **Status messages** - Activation status shows in UI instead of popups
10. **App icon support** - Installer uses custom icon

## v2.0.1 - Namespace Conflict Fixes

Fixed WinForms/WPF namespace conflicts introduced by `UseWindowsForms=true` in csproj:

- **AdminWindow.xaml.cs** - Rewrote with using aliases to resolve ambiguous types:
  - `WpfColor = System.Windows.Media.Color`
  - `WpfMessageBox = System.Windows.MessageBox`
  - `WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog`
  - `WinForms = System.Windows.Forms` (for color picker dialog)
- **LoginWindow.xaml.cs** - Fixed `KeyEventArgs` ambiguity with fully qualified `System.Windows.Input.KeyEventArgs`
- **App.xaml.cs** - Fixed `Application` ambiguity with fully qualified `System.Windows.Application`
- **ProcessSelectorDialog.xaml.cs** - Fixed `MessageBox` ambiguity with fully qualified `System.Windows.MessageBox`

## v2.0.2 - Features & Fixes

### New Features
- **New Action Types**: Added "Open PDF Document" and "Open Picture" for custom buttons
- **Updated Labels**: "Run Executable" → "Start Program", "Open URL" → "Open Website"
- **Header Icon**: Admin panel now displays app icon in header
- **Video Progress**: Download now shows real-time progress percentage

### Bug Fixes
- **Video Download Hanging**: Fixed by reading stdout/stderr asynchronously to prevent buffer deadlock
- **Scrollbar Overlap**: Added padding to all ScrollViewer panels
- **Activation Server**: Fixed reactivation bug for deactivated licenses
- **App Icons**: All executables now have proper application icons

### Documentation
- **Admin Guide**: Created comprehensive HTML guide with screenshots (`snaps/SimLock_Admin_Guide.html`)
- **Video Script**: Created video production script and storyboard (`snaps/SimLock_Video_Script.md`)
- **Screenshots**: Added 13 screenshots of all admin and lock screen states

## License
Copyright (c) NeutroCorp LLC. All rights reserved.

## Support
- Website: https://www.neutrocorp.com
- Email: support@neutrocorp.com

---

## Next Context Prompt

```
I'm continuing work on SimLock, a .NET 8 WPF kiosk lock screen for golf simulators.

Project location: /home/csolaiman/SimLock
Activation server: 192.168.88.197 (/home/csolaiman/activation-server/)
Windows share: //192.168.88.156/Users/cstk421/Downloads/Dev (mounted at /mnt/windev on activation server)

Key files:
- src/SimLock.Admin/AdminWindow.xaml(.cs) - Admin panel
- src/SimLock.Locker/MainWindow.xaml(.cs) - Lock screen
- src/SimLock.Common/AppConfig.cs - Configuration
- activation-server/app.py - License server
- installer/SimLock_Setup.iss - Inno Setup installer
- snaps/SimLock_Admin_Guide.html - User documentation

GitHub: https://github.com/cstk421/SimWoods-Golf
SSH to activation server: ssh -i ~/.ssh/dms-deploy csolaiman@192.168.88.197

Last session (v2.0.2) completed:
- Added new action types: Open PDF Document, Open Picture
- Updated dropdown labels: "Start Program", "Open Website"
- Fixed video download hanging (async stdout/stderr reading)
- Fixed scrollbar overlap in admin panel tabs
- Fixed activation server reactivation bug
- Added app icons to all executables
- Created Admin Guide HTML with screenshots
- Created video production script/storyboard

Build commands:
  dotnet publish -c Release -o publish
  Then run Inno Setup on installer/SimLock_Setup.iss

To deploy files to Windows share:
  scp -i ~/.ssh/dms-deploy <file> csolaiman@192.168.88.197:/mnt/windev/<path>

Please read summary.md for full project context.
```
