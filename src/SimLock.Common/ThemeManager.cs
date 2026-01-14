using System.Windows;
using System.Windows.Media;

namespace SimLock.Common;

/// <summary>
/// Manages dynamic theme application for SimLock applications.
/// Updates application resources at runtime based on configuration.
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// Applies theme settings from config to the application's resources.
    /// </summary>
    public static void ApplyTheme(Application app, AppConfig config)
    {
        if (app?.Resources == null) return;

        var resources = app.Resources;

        // Update color resources
        UpdateColor(resources, "PrimaryColor", config.ThemePrimaryColor);
        UpdateColor(resources, "PrimaryLightColor", config.ThemeSecondaryColor);
        UpdateColor(resources, "PrimaryDarkColor", DarkenColor(config.ThemePrimaryColor, 0.3));
        UpdateColor(resources, "AccentColor", config.ThemeAccentColor);
        UpdateColor(resources, "BackgroundColor", config.ThemeBackgroundColor);
        UpdateColor(resources, "SurfaceColor", config.ThemeSurfaceColor);
        UpdateColor(resources, "TextPrimaryColor", config.ThemeTextPrimaryColor);
        UpdateColor(resources, "TextSecondaryColor", config.ThemeTextSecondaryColor);
        UpdateColor(resources, "ErrorColor", config.ThemeErrorColor);

        // Update brush resources
        UpdateBrush(resources, "PrimaryBrush", config.ThemePrimaryColor);
        UpdateBrush(resources, "PrimaryLightBrush", config.ThemeSecondaryColor);
        UpdateBrush(resources, "PrimaryDarkBrush", DarkenColor(config.ThemePrimaryColor, 0.3));
        UpdateBrush(resources, "AccentBrush", config.ThemeAccentColor);
        UpdateBrush(resources, "BackgroundBrush", config.ThemeBackgroundColor);
        UpdateBrush(resources, "SurfaceBrush", config.ThemeSurfaceColor);
        UpdateBrush(resources, "TextPrimaryBrush", config.ThemeTextPrimaryColor);
        UpdateBrush(resources, "TextSecondaryBrush", config.ThemeTextSecondaryColor);
        UpdateBrush(resources, "ErrorBrush", config.ThemeErrorColor);

        // Update font family
        if (!string.IsNullOrEmpty(config.ThemeFontFamily))
        {
            try
            {
                var fontFamily = new FontFamily(config.ThemeFontFamily);
                resources["AppFontFamily"] = fontFamily;
            }
            catch
            {
                // Keep default font if specified font is invalid
            }
        }
    }

    /// <summary>
    /// Applies theme to a specific window's resources (for preview).
    /// </summary>
    public static void ApplyThemeToWindow(Window window, AppConfig config)
    {
        if (window?.Resources == null) return;

        var resources = window.Resources;

        UpdateBrush(resources, "PrimaryBrush", config.ThemePrimaryColor);
        UpdateBrush(resources, "PrimaryLightBrush", config.ThemeSecondaryColor);
        UpdateBrush(resources, "AccentBrush", config.ThemeAccentColor);
        UpdateBrush(resources, "BackgroundBrush", config.ThemeBackgroundColor);
        UpdateBrush(resources, "SurfaceBrush", config.ThemeSurfaceColor);
        UpdateBrush(resources, "TextPrimaryBrush", config.ThemeTextPrimaryColor);
        UpdateBrush(resources, "TextSecondaryBrush", config.ThemeTextSecondaryColor);
        UpdateBrush(resources, "ErrorBrush", config.ThemeErrorColor);
    }

    /// <summary>
    /// Resets theme to default golf-themed colors.
    /// </summary>
    public static void ResetToDefaults(AppConfig config)
    {
        config.ThemePrimaryColor = "#2E7D32";
        config.ThemeSecondaryColor = "#60AD5E";
        config.ThemeAccentColor = "#FFD700";
        config.ThemeBackgroundColor = "#1A2F1A";
        config.ThemeSurfaceColor = "#2D4A2D";
        config.ThemeTextPrimaryColor = "#FFFFFF";
        config.ThemeTextSecondaryColor = "#C8D6C8";
        config.ThemeErrorColor = "#FF5252";
        config.ThemeFontFamily = "Segoe UI";
    }

    /// <summary>
    /// Parses a hex color string to a Color object.
    /// </summary>
    public static Color ParseColor(string hexColor)
    {
        try
        {
            if (string.IsNullOrEmpty(hexColor))
                return Colors.Gray;

            return (Color)ColorConverter.ConvertFromString(hexColor);
        }
        catch
        {
            return Colors.Gray;
        }
    }

    /// <summary>
    /// Validates if a string is a valid hex color.
    /// </summary>
    public static bool IsValidHexColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return false;

        try
        {
            ColorConverter.ConvertFromString(hexColor);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Darkens a color by a percentage (0.0 to 1.0).
    /// </summary>
    public static string DarkenColor(string hexColor, double amount)
    {
        try
        {
            var color = ParseColor(hexColor);
            var r = (byte)Math.Max(0, color.R * (1 - amount));
            var g = (byte)Math.Max(0, color.G * (1 - amount));
            var b = (byte)Math.Max(0, color.B * (1 - amount));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch
        {
            return hexColor;
        }
    }

    /// <summary>
    /// Lightens a color by a percentage (0.0 to 1.0).
    /// </summary>
    public static string LightenColor(string hexColor, double amount)
    {
        try
        {
            var color = ParseColor(hexColor);
            var r = (byte)Math.Min(255, color.R + (255 - color.R) * amount);
            var g = (byte)Math.Min(255, color.G + (255 - color.G) * amount);
            var b = (byte)Math.Min(255, color.B + (255 - color.B) * amount);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch
        {
            return hexColor;
        }
    }

    private static void UpdateColor(ResourceDictionary resources, string key, string hexColor)
    {
        try
        {
            var color = ParseColor(hexColor);
            if (resources.Contains(key))
            {
                resources[key] = color;
            }
            else
            {
                resources.Add(key, color);
            }
        }
        catch
        {
            // Ignore invalid colors
        }
    }

    private static void UpdateBrush(ResourceDictionary resources, string key, string hexColor)
    {
        try
        {
            var color = ParseColor(hexColor);
            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Improves performance

            if (resources.Contains(key))
            {
                resources[key] = brush;
            }
            else
            {
                resources.Add(key, brush);
            }
        }
        catch
        {
            // Ignore invalid colors
        }
    }
}
