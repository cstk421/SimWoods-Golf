using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimLock.Common;

public class AppConfig
{
    public string UnlockCode { get; set; } = "1234";
    public string VideoUrl { get; set; } = "https://youtu.be/hI-G8T8PoOA";
    public string LocalVideoPath { get; set; } = "Videos/tutorial.mp4";
    public string LogoPath { get; set; } = "Assets/logo.png";
    public string SplashImagePath { get; set; } = "Assets/splash.png";
    public string SplashTitle { get; set; } = "Welcome to SimWoods Golf";
    public string SplashSubtitle { get; set; } = "Your Premium Golf Simulation Experience";
    public string MonitoredProcessName { get; set; } = "gspro";
    public string AdminPassword { get; set; } = "admin123";

    // Configurable screen text
    public string PinScreenTitle { get; set; } = "Enter Your 4-Digit Code";
    public string CustomMessage { get; set; } = "";
    public bool ShowCustomMessage { get; set; } = false;

    // ============ SPLASH BACKGROUND ============
    public string SplashBackgroundImagePath { get; set; } = "";
    public bool UseSplashBackgroundImage { get; set; } = false;
    public double SplashTextBoxOpacity { get; set; } = 0.85;

    // ============ BUTTON VISIBILITY ============
    public bool ShowReturningGolferButton { get; set; } = true;
    public bool ShowTutorialButton { get; set; } = true;
    public bool ShowCustomButton1 { get; set; } = false;
    public bool ShowCustomButton2 { get; set; } = false;

    // ============ CUSTOM BUTTON 1 ============
    public string CustomButton1Label { get; set; } = "Custom Action 1";
    public string CustomButton1ActionType { get; set; } = "RunExecutable"; // RunExecutable, OpenUrl, PlayLocalVideo
    public string CustomButton1Target { get; set; } = "";

    // ============ CUSTOM BUTTON 2 ============
    public string CustomButton2Label { get; set; } = "Custom Action 2";
    public string CustomButton2ActionType { get; set; } = "RunExecutable";
    public string CustomButton2Target { get; set; } = "";

    // ============ THEME SETTINGS ============
    public string ThemePrimaryColor { get; set; } = "#2E7D32";
    public string ThemeSecondaryColor { get; set; } = "#60AD5E";
    public string ThemeAccentColor { get; set; } = "#FFD700";
    public string ThemeBackgroundColor { get; set; } = "#1A2F1A";
    public string ThemeSurfaceColor { get; set; } = "#2D4A2D";
    public string ThemeTextPrimaryColor { get; set; } = "#FFFFFF";
    public string ThemeTextSecondaryColor { get; set; } = "#C8D6C8";
    public string ThemeErrorColor { get; set; } = "#FF5252";
    public string ThemeFontFamily { get; set; } = "Segoe UI";

    // ============ ACTIVATION SETTINGS ============
    public bool IsActivated { get; set; } = false;
    public string ActivationEmail { get; set; } = "";
    public string MachineId { get; set; } = "";
    public string LicenseKey { get; set; } = "";
    public DateTime? ActivationDate { get; set; } = null;
    public string ActivationServerUrl { get; set; } = "https://activation.neutrocorp.com:8443";

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SimLock", "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
        }

        var config = new AppConfig();
        config.Save();
        return config;
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    public string GetAbsoluteVideoPath()
    {
        if (Path.IsPathRooted(LocalVideoPath))
            return LocalVideoPath;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalVideoPath);
    }

    public string GetAbsoluteLogoPath()
    {
        if (Path.IsPathRooted(LogoPath))
            return LogoPath;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogoPath);
    }

    public string GetAbsoluteSplashImagePath()
    {
        if (Path.IsPathRooted(SplashImagePath))
            return SplashImagePath;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SplashImagePath);
    }

    public string GetAbsoluteSplashBackgroundPath()
    {
        if (string.IsNullOrEmpty(SplashBackgroundImagePath))
            return "";
        if (Path.IsPathRooted(SplashBackgroundImagePath))
            return SplashBackgroundImagePath;
        return Path.Combine(GetAssetsDirectory(), SplashBackgroundImagePath);
    }

    public string GetAbsoluteCustomButtonTarget(string target)
    {
        if (string.IsNullOrEmpty(target))
            return "";
        if (Path.IsPathRooted(target) || target.StartsWith("http://") || target.StartsWith("https://"))
            return target;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, target);
    }

    /// <summary>
    /// Gets a writable data directory for downloads and user data.
    /// Uses ProgramData\SimLock which has write permissions.
    /// </summary>
    public static string GetDataDirectory()
    {
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SimLock");

        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Create subdirectories
        var videosDir = Path.Combine(dataDir, "Videos");
        var assetsDir = Path.Combine(dataDir, "Assets");

        if (!Directory.Exists(videosDir))
            Directory.CreateDirectory(videosDir);
        if (!Directory.Exists(assetsDir))
            Directory.CreateDirectory(assetsDir);

        return dataDir;
    }

    /// <summary>
    /// Gets the path to store yt-dlp executable.
    /// </summary>
    public static string GetYtDlpPath()
    {
        return Path.Combine(GetDataDirectory(), "yt-dlp.exe");
    }

    /// <summary>
    /// Gets the writable videos directory.
    /// </summary>
    public static string GetVideosDirectory()
    {
        return Path.Combine(GetDataDirectory(), "Videos");
    }

    /// <summary>
    /// Gets the writable assets directory.
    /// </summary>
    public static string GetAssetsDirectory()
    {
        return Path.Combine(GetDataDirectory(), "Assets");
    }

    /// <summary>
    /// Gets the path to the config file.
    /// </summary>
    public static string GetConfigFilePath()
    {
        return ConfigPath;
    }
}
