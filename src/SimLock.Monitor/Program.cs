using System.Diagnostics;
using SimLock.Common;

namespace SimLock.Monitor;

static class Program
{
    private static NotifyIcon? _trayIcon;
    private static AppConfig _config = null!;
    private static System.Threading.Timer? _pollTimer;
    private static FileSystemWatcher? _configWatcher;
    private static bool _processWasRunning = false;
    private static bool _lockScreenLaunched = false;
    private static DateTime _lastConfigReload = DateTime.MinValue;

    [STAThread]
    static void Main()
    {
        // Prevent multiple instances
        using var mutex = new Mutex(true, "SimLockMonitor_Mutex", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("SimLock Monitor is already running.", "SimLock Monitor",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        _config = AppConfig.Load();

        InitializeTrayIcon();
        StartProcessMonitoring();
        StartConfigWatcher();

        Application.Run();

        // Cleanup
        _pollTimer?.Dispose();
        _configWatcher?.Dispose();
        _trayIcon?.Dispose();
    }

    private static void InitializeTrayIcon()
    {
        // Use Shield icon (lock/security symbol)
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Text = "SimLock Monitor",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();

        contextMenu.Items.Add("Open Admin Panel", null, (s, e) => OpenAdminPanel());
        contextMenu.Items.Add("Launch Lock Screen", null, (s, e) => LaunchLockScreen());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Reload Config", null, (s, e) => ReloadConfig());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) => OpenAdminPanel();

        ShowNotification("SimLock Monitor", $"Monitoring for: {_config.MonitoredProcessName}.exe");
    }

    private static void StartProcessMonitoring()
    {
        // Poll every 2 seconds
        _pollTimer = new System.Threading.Timer(
            CheckForProcess,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));
    }

    private static void StartConfigWatcher()
    {
        try
        {
            var configPath = AppConfig.GetConfigFilePath();
            var configDir = Path.GetDirectoryName(configPath);
            var configFile = Path.GetFileName(configPath);

            if (string.IsNullOrEmpty(configDir) || !Directory.Exists(configDir))
                return;

            _configWatcher = new FileSystemWatcher(configDir, configFile)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _configWatcher.Changed += OnConfigChanged;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting up config watcher: {ex.Message}");
        }
    }

    private static void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce - FileSystemWatcher can fire multiple times
        if ((DateTime.Now - _lastConfigReload).TotalSeconds < 2)
            return;

        _lastConfigReload = DateTime.Now;

        try
        {
            // Small delay to ensure file write is complete
            Thread.Sleep(500);

            var oldProcessName = _config.MonitoredProcessName;
            _config = AppConfig.Load();

            if (oldProcessName != _config.MonitoredProcessName)
            {
                // Reset process tracking state when monitored process changes
                _processWasRunning = false;
                _lockScreenLaunched = false;
                ShowNotification("SimLock Monitor", $"Config updated. Now monitoring: {_config.MonitoredProcessName}.exe");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reloading config: {ex.Message}");
        }
    }

    private static void CheckForProcess(object? state)
    {
        try
        {
            var processName = _config.MonitoredProcessName;
            var processes = Process.GetProcessesByName(processName);
            var isRunning = processes.Length > 0;

            // Detect when process starts (transition from not running to running)
            if (isRunning && !_processWasRunning && !_lockScreenLaunched)
            {
                _lockScreenLaunched = true;
                LaunchLockScreen();
            }

            // Reset lock screen flag when process stops
            if (!isRunning && _processWasRunning)
            {
                _lockScreenLaunched = false;
            }

            _processWasRunning = isRunning;

            // Dispose process objects
            foreach (var p in processes)
            {
                p.Dispose();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking process: {ex.Message}");
        }
    }

    private static void LaunchLockScreen()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exePath = Path.Combine(baseDir, "SimLock.exe");

            if (!File.Exists(exePath))
            {
                // Development fallback - look in Locker project
                var solutionDir = FindSolutionDirectory(baseDir);
                if (solutionDir != null)
                {
                    exePath = Path.Combine(solutionDir, "src", "SimLock.Locker", "bin", "Debug", "net8.0-windows", "SimLock.exe");
                }
            }

            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
            }
            else
            {
                ShowNotification("SimLock Error", "SimLock.exe not found!");
            }
        }
        catch (Exception ex)
        {
            ShowNotification("SimLock Error", $"Failed to launch: {ex.Message}");
        }
    }

    private static void OpenAdminPanel()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exePath = Path.Combine(baseDir, "SimLock.Admin.exe");

            if (!File.Exists(exePath))
            {
                // Development fallback
                var solutionDir = FindSolutionDirectory(baseDir);
                if (solutionDir != null)
                {
                    exePath = Path.Combine(solutionDir, "src", "SimLock.Admin", "bin", "Debug", "net8.0-windows", "SimLock.Admin.exe");
                }
            }

            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("SimLock.Admin.exe not found!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open admin panel: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void ReloadConfig()
    {
        _config = AppConfig.Load();
        ShowNotification("SimLock Monitor", $"Config reloaded. Monitoring: {_config.MonitoredProcessName}.exe");
    }

    private static void ShowNotification(string title, string message)
    {
        _trayIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
    }

    private static void ExitApplication()
    {
        _pollTimer?.Dispose();
        _configWatcher?.Dispose();
        if (_trayIcon != null)
            _trayIcon.Visible = false;
        Application.Exit();
    }

    private static string? FindSolutionDirectory(string startDir)
    {
        var dir = startDir;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "SimLock.sln")))
                return dir;

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return null;
    }
}
