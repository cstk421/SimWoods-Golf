using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SimLock.Common;

namespace SimLock.Admin;

public partial class AdminWindow : Window
{
    private AppConfig _config;
    private bool _isDownloading = false;
    private ActivationService? _activationService;

    public AdminWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load();
        _activationService = new ActivationService(_config.ActivationServerUrl);
        LoadSettings();
        UpdateActivationUI();
    }

    private void LoadSettings()
    {
        // Security
        UnlockCodeInput.Text = _config.UnlockCode;
        AdminPasswordInput.Text = _config.AdminPassword;

        // Video
        VideoUrlInput.Text = _config.VideoUrl;
        VideoPathInput.Text = _config.LocalVideoPath;

        // Splash Screen
        SplashTitleInput.Text = _config.SplashTitle;
        SplashSubtitleInput.Text = _config.SplashSubtitle;
        SplashImageInput.Text = _config.SplashImagePath;
        UseSplashBackgroundCheckbox.IsChecked = _config.UseSplashBackgroundImage;
        SplashBackgroundInput.Text = _config.SplashBackgroundImagePath;
        SplashTextOpacitySlider.Value = _config.SplashTextBoxOpacity;

        // Branding
        LogoPathInput.Text = _config.LogoPath;

        // Process Monitor
        MonitoredProcessInput.Text = _config.MonitoredProcessName;

        // Screen Customization
        PinScreenTitleInput.Text = _config.PinScreenTitle;
        CustomMessageInput.Text = _config.CustomMessage;
        ShowCustomMessageCheckbox.IsChecked = _config.ShowCustomMessage;

        // Button Configuration
        ShowReturningGolferCheckbox.IsChecked = _config.ShowReturningGolferButton;
        ShowTutorialCheckbox.IsChecked = _config.ShowTutorialButton;
        ShowCustomButton1Checkbox.IsChecked = _config.ShowCustomButton1;
        CustomButton1LabelInput.Text = _config.CustomButton1Label;
        SetComboBoxByTag(CustomButton1ActionCombo, _config.CustomButton1ActionType);
        CustomButton1TargetInput.Text = _config.CustomButton1Target;
        ShowCustomButton2Checkbox.IsChecked = _config.ShowCustomButton2;
        CustomButton2LabelInput.Text = _config.CustomButton2Label;
        SetComboBoxByTag(CustomButton2ActionCombo, _config.CustomButton2ActionType);
        CustomButton2TargetInput.Text = _config.CustomButton2Target;

        // Theme Settings
        ThemePrimaryColorInput.Text = _config.ThemePrimaryColor;
        ThemeSecondaryColorInput.Text = _config.ThemeSecondaryColor;
        ThemeAccentColorInput.Text = _config.ThemeAccentColor;
        ThemeBackgroundColorInput.Text = _config.ThemeBackgroundColor;
        ThemeSurfaceColorInput.Text = _config.ThemeSurfaceColor;
        ThemeTextPrimaryColorInput.Text = _config.ThemeTextPrimaryColor;
        ThemeTextSecondaryColorInput.Text = _config.ThemeTextSecondaryColor;
        SetComboBoxByContent(ThemeFontFamilyCombo, _config.ThemeFontFamily);

        // Activation
        ActivationEmailInput.Text = _config.ActivationEmail;

        LoadLogoPreview();
        UpdateColorPreviews();
    }

    private void SetComboBoxByTag(ComboBox comboBox, string tag)
    {
        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (item.Tag?.ToString() == tag)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
        comboBox.SelectedIndex = 0;
    }

    private void SetComboBoxByContent(ComboBox comboBox, string content)
    {
        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (item.Content?.ToString() == content)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
        comboBox.SelectedIndex = 0;
    }

    private string GetComboBoxTag(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "RunExecutable";
    }

    private string GetComboBoxContent(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Segoe UI";
    }

    private void LoadLogoPreview()
    {
        try
        {
            var logoPath = _config.GetAbsoluteLogoPath();
            if (File.Exists(logoPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                bitmap.EndInit();
                LogoPreview.Source = bitmap;
            }
        }
        catch
        {
            // Ignore preview errors
        }
    }

    private void UpdateColorPreviews()
    {
        UpdateColorPreview(ThemePrimaryColorInput.Text, ThemePrimaryColorPreview);
        UpdateColorPreview(ThemeSecondaryColorInput.Text, ThemeSecondaryColorPreview);
        UpdateColorPreview(ThemeAccentColorInput.Text, ThemeAccentColorPreview);
        UpdateColorPreview(ThemeBackgroundColorInput.Text, ThemeBackgroundColorPreview);
        UpdateColorPreview(ThemeSurfaceColorInput.Text, ThemeSurfaceColorPreview);
        UpdateColorPreview(ThemeTextPrimaryColorInput.Text, ThemeTextPrimaryColorPreview);
        UpdateColorPreview(ThemeTextSecondaryColorInput.Text, ThemeTextSecondaryColorPreview);
    }

    private void UpdateColorPreview(string hexColor, Border preview)
    {
        try
        {
            var color = ThemeManager.ParseColor(hexColor);
            preview.Background = new SolidColorBrush(color);
        }
        catch
        {
            preview.Background = new SolidColorBrush(Colors.Gray);
        }
    }

    private void UpdateColorPreviews_Click(object sender, RoutedEventArgs e)
    {
        UpdateColorPreviews();
    }

    #region Activation

    private void UpdateActivationUI()
    {
        var machineId = MachineIdentifier.GetMachineId();
        MachineIdDisplay.Text = $"Machine ID: {machineId}";

        if (_config.IsActivated)
        {
            ActivationStatusText.Text = "Activated";
            ActivationStatusText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            ActivationEmailDisplay.Text = _config.ActivationEmail;
            LicenseBalanceText.Text = "";
            ActivationInputPanel.Visibility = Visibility.Collapsed;
            ActivatedPanel.Visibility = Visibility.Visible;
        }
        else
        {
            ActivationStatusText.Text = "Not Activated";
            ActivationStatusText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
            LicenseBalanceText.Text = "";
            ActivationInputPanel.Visibility = Visibility.Visible;
            ActivatedPanel.Visibility = Visibility.Collapsed;
        }
    }

    private async void CheckLicense_Click(object sender, RoutedEventArgs e)
    {
        var email = ActivationEmailInput.Text.Trim();

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ActivateButton.IsEnabled = false;
        ActivateButton.Content = "Checking...";

        try
        {
            var result = await _activationService!.CheckAndActivateAsync(email);

            if (result.Success)
            {
                _config.IsActivated = true;
                _config.ActivationEmail = email;
                _config.LicenseKey = result.LicenseKey ?? "";
                _config.MachineId = result.MachineId ?? MachineIdentifier.GetMachineId();
                _config.ActivationDate = DateTime.Now;
                _config.Save();

                UpdateActivationUI();

                var balanceMsg = result.RemainingActivations.HasValue
                    ? $"\n\nRemaining activations: {result.RemainingActivations}"
                    : "";

                MessageBox.Show($"License activated successfully!{balanceMsg}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Show balance if available
                if (result.RemainingActivations.HasValue)
                {
                    LicenseBalanceText.Text = $"(Available: {result.RemainingActivations})";
                }

                MessageBox.Show($"{result.Message}", "Activation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection error: {ex.Message}\n\nPlease check your internet connection and try again.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ActivateButton.IsEnabled = true;
            ActivateButton.Content = "Check License";
        }
    }

    private async void Deactivate_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to deactivate this machine?\n\nThis will free up one activation for use on another machine.",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            var deactivateResult = await _activationService!.DeactivateAsync(
                _config.ActivationEmail,
                _config.LicenseKey,
                _config.MachineId);

            if (deactivateResult.Success)
            {
                _config.IsActivated = false;
                _config.ActivationDate = null;
                _config.LicenseKey = "";
                _config.Save();

                UpdateActivationUI();
                MessageBox.Show("Machine deactivated successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Deactivation failed: {deactivateResult.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Deactivation error: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SupportEmail_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "mailto:support@neutrocorp.com",
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void Website_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.neutrocorp.com",
                UseShellExecute = true
            });
        }
        catch { }
    }

    #endregion

    #region Theme

    private void ResetTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemePrimaryColorInput.Text = "#2E7D32";
        ThemeSecondaryColorInput.Text = "#60AD5E";
        ThemeAccentColorInput.Text = "#FFD700";
        ThemeBackgroundColorInput.Text = "#1A2F1A";
        ThemeSurfaceColorInput.Text = "#2D4A2D";
        ThemeTextPrimaryColorInput.Text = "#FFFFFF";
        ThemeTextSecondaryColorInput.Text = "#C8D6C8";
        ThemeFontFamilyCombo.SelectedIndex = 0;
        UpdateColorPreviews();
    }

    #endregion

    #region Custom Buttons

    private void BrowseCustomButton1Target_Click(object sender, RoutedEventArgs e)
    {
        BrowseCustomButtonTarget(CustomButton1ActionCombo, CustomButton1TargetInput);
    }

    private void BrowseCustomButton2Target_Click(object sender, RoutedEventArgs e)
    {
        BrowseCustomButtonTarget(CustomButton2ActionCombo, CustomButton2TargetInput);
    }

    private void BrowseCustomButtonTarget(ComboBox actionCombo, TextBox targetInput)
    {
        var actionType = GetComboBoxTag(actionCombo);

        switch (actionType)
        {
            case "RunExecutable":
                var exeDialog = new OpenFileDialog
                {
                    Filter = "Executable Files|*.exe|All Files|*.*",
                    Title = "Select Executable"
                };
                if (exeDialog.ShowDialog() == true)
                {
                    targetInput.Text = exeDialog.FileName;
                }
                break;

            case "PlayLocalVideo":
                var videoDialog = new OpenFileDialog
                {
                    Filter = "Video Files|*.mp4;*.avi;*.mkv;*.wmv;*.mov|All Files|*.*",
                    Title = "Select Video File"
                };
                if (videoDialog.ShowDialog() == true)
                {
                    targetInput.Text = videoDialog.FileName;
                }
                break;

            case "OpenUrl":
                MessageBox.Show("Please enter the URL directly in the text field.",
                    "URL Input", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
        }
    }

    #endregion

    #region Splash Background

    private void BrowseSplashBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
            Title = "Select Splash Background Image"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var assetsDir = AppConfig.GetAssetsDirectory();
                var fileName = "splash_background" + Path.GetExtension(dialog.FileName);
                var destPath = Path.Combine(assetsDir, fileName);
                File.Copy(dialog.FileName, destPath, overwrite: true);
                SplashBackgroundInput.Text = destPath;
                _config.SplashBackgroundImagePath = destPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SplashBackgroundInput.Text = dialog.FileName;
            }
        }
    }

    #endregion

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        // Validate unlock code
        if (string.IsNullOrWhiteSpace(UnlockCodeInput.Text) || UnlockCodeInput.Text.Length != 4 ||
            !UnlockCodeInput.Text.All(char.IsDigit))
        {
            MessageBox.Show("Unlock code must be exactly 4 digits.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validate admin password
        if (string.IsNullOrWhiteSpace(AdminPasswordInput.Text))
        {
            MessageBox.Show("Admin password cannot be empty.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Security
        _config.UnlockCode = UnlockCodeInput.Text;
        _config.AdminPassword = AdminPasswordInput.Text;

        // Video
        _config.VideoUrl = VideoUrlInput.Text;
        _config.LocalVideoPath = VideoPathInput.Text;

        // Splash Screen
        _config.SplashTitle = SplashTitleInput.Text;
        _config.SplashSubtitle = SplashSubtitleInput.Text;
        _config.SplashImagePath = SplashImageInput.Text;
        _config.UseSplashBackgroundImage = UseSplashBackgroundCheckbox.IsChecked ?? false;
        _config.SplashBackgroundImagePath = SplashBackgroundInput.Text;
        _config.SplashTextBoxOpacity = SplashTextOpacitySlider.Value;

        // Branding
        _config.LogoPath = LogoPathInput.Text;

        // Process Monitor
        _config.MonitoredProcessName = MonitoredProcessInput.Text;

        // Screen Customization
        _config.PinScreenTitle = PinScreenTitleInput.Text;
        _config.CustomMessage = CustomMessageInput.Text;
        _config.ShowCustomMessage = ShowCustomMessageCheckbox.IsChecked ?? false;

        // Button Configuration
        _config.ShowReturningGolferButton = ShowReturningGolferCheckbox.IsChecked ?? true;
        _config.ShowTutorialButton = ShowTutorialCheckbox.IsChecked ?? true;
        _config.ShowCustomButton1 = ShowCustomButton1Checkbox.IsChecked ?? false;
        _config.CustomButton1Label = CustomButton1LabelInput.Text;
        _config.CustomButton1ActionType = GetComboBoxTag(CustomButton1ActionCombo);
        _config.CustomButton1Target = CustomButton1TargetInput.Text;
        _config.ShowCustomButton2 = ShowCustomButton2Checkbox.IsChecked ?? false;
        _config.CustomButton2Label = CustomButton2LabelInput.Text;
        _config.CustomButton2ActionType = GetComboBoxTag(CustomButton2ActionCombo);
        _config.CustomButton2Target = CustomButton2TargetInput.Text;

        // Theme Settings
        _config.ThemePrimaryColor = ThemePrimaryColorInput.Text;
        _config.ThemeSecondaryColor = ThemeSecondaryColorInput.Text;
        _config.ThemeAccentColor = ThemeAccentColorInput.Text;
        _config.ThemeBackgroundColor = ThemeBackgroundColorInput.Text;
        _config.ThemeSurfaceColor = ThemeSurfaceColorInput.Text;
        _config.ThemeTextPrimaryColor = ThemeTextPrimaryColorInput.Text;
        _config.ThemeTextSecondaryColor = ThemeTextSecondaryColorInput.Text;
        _config.ThemeFontFamily = GetComboBoxContent(ThemeFontFamilyCombo);

        _config.Save();
        MessageBox.Show("Settings saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BrowseVideo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.avi;*.mkv;*.wmv;*.mov|All Files|*.*",
            Title = "Select Tutorial Video"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var videosDir = AppConfig.GetVideosDirectory();
                var destPath = Path.Combine(videosDir, "tutorial.mp4");
                File.Copy(dialog.FileName, destPath, overwrite: true);
                VideoPathInput.Text = destPath;
                _config.LocalVideoPath = destPath;
                MessageBox.Show("Video copied successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying video: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                VideoPathInput.Text = dialog.FileName;
            }
        }
    }

    private void BrowseSplashImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
            Title = "Select Splash Logo Image"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var assetsDir = AppConfig.GetAssetsDirectory();
                var fileName = Path.GetFileName(dialog.FileName);
                var destPath = Path.Combine(assetsDir, "splash" + Path.GetExtension(fileName));
                File.Copy(dialog.FileName, destPath, overwrite: true);
                SplashImageInput.Text = destPath;
                _config.SplashImagePath = destPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SplashImageInput.Text = dialog.FileName;
            }
        }
    }

    private void BrowseLogo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
            Title = "Select Logo Image"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var assetsDir = AppConfig.GetAssetsDirectory();
                var fileName = Path.GetFileName(dialog.FileName);
                var destPath = Path.Combine(assetsDir, "logo" + Path.GetExtension(fileName));
                File.Copy(dialog.FileName, destPath, overwrite: true);
                LogoPathInput.Text = destPath;
                _config.LogoPath = destPath;
                LoadLogoPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying logo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogoPathInput.Text = dialog.FileName;
                _config.LogoPath = dialog.FileName;
                LoadLogoPreview();
            }
        }
    }

    private async void DownloadVideo_Click(object sender, RoutedEventArgs e)
    {
        if (_isDownloading)
        {
            MessageBox.Show("Download already in progress.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var url = VideoUrlInput.Text;
        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("Please enter a video URL first.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _isDownloading = true;
        DownloadStatusText.Text = "Preparing download...";
        DownloadProgress.Visibility = Visibility.Visible;
        DownloadProgress.IsIndeterminate = true;

        try
        {
            var videosDir = AppConfig.GetVideosDirectory();
            var outputPath = Path.Combine(videosDir, "tutorial.mp4");

            var ytdlpPath = AppConfig.GetYtDlpPath();
            if (!File.Exists(ytdlpPath))
            {
                DownloadStatusText.Text = "Downloading yt-dlp...";
                await DownloadYtDlpAsync(ytdlpPath);
            }

            var ffmpegPath = Path.Combine(AppConfig.GetDataDirectory(), "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
            {
                DownloadStatusText.Text = "Downloading ffmpeg...";
                await DownloadFfmpegAsync(ffmpegPath);
            }

            DownloadStatusText.Text = "Downloading video...";

            var success = await DownloadVideoAsync(ytdlpPath, url, outputPath);

            if (success)
            {
                VideoPathInput.Text = outputPath;
                _config.LocalVideoPath = outputPath;
                _config.Save();
                DownloadStatusText.Text = "Download complete!";
                MessageBox.Show("Video downloaded successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                DownloadStatusText.Text = "Download failed.";
            }
        }
        catch (Exception ex)
        {
            DownloadStatusText.Text = "Download failed.";
            MessageBox.Show($"Error downloading video: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isDownloading = false;
            DownloadProgress.Visibility = Visibility.Collapsed;
        }
    }

    private async Task DownloadYtDlpAsync(string targetPath)
    {
        var downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

        using var client = new System.Net.Http.HttpClient();
        var bytes = await client.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(targetPath, bytes);
    }

    private async Task DownloadFfmpegAsync(string targetPath)
    {
        var downloadUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
        var tempZip = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");
        var extractDir = Path.Combine(Path.GetTempPath(), "ffmpeg_extract");

        using var client = new System.Net.Http.HttpClient();
        client.Timeout = TimeSpan.FromMinutes(10);

        var bytes = await client.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(tempZip, bytes);

        if (Directory.Exists(extractDir))
            Directory.Delete(extractDir, true);

        ZipFile.ExtractToDirectory(tempZip, extractDir);

        var ffmpegExe = Directory.GetFiles(extractDir, "ffmpeg.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (ffmpegExe != null)
        {
            File.Copy(ffmpegExe, targetPath, overwrite: true);
        }

        try
        {
            File.Delete(tempZip);
            Directory.Delete(extractDir, true);
        }
        catch { }
    }

    private async Task<bool> DownloadVideoAsync(string ytdlpPath, string url, string outputPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                var ffmpegDir = AppConfig.GetDataDirectory();
                var formatArgs = "-f \"bestvideo[ext=mp4][vcodec^=avc1]+bestaudio[ext=m4a]/bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" --merge-output-format mp4";

                var psi = new ProcessStartInfo
                {
                    FileName = ytdlpPath,
                    Arguments = $"--no-playlist --ffmpeg-location \"{ffmpegDir}\" {formatArgs} -o \"{outputPath}\" \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null) return false;

                process.WaitForExit();
                return File.Exists(outputPath);
            }
            catch
            {
                return false;
            }
        });
    }

    private void TestLockScreen_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings_Click(sender, e);

        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimLock.exe");

        if (!File.Exists(exePath))
        {
            var solutionDir = FindSolutionDirectory();
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
            MessageBox.Show(
                "SimLock.exe not found. Please build the SimLock.Locker project first.",
                "Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private string? FindSolutionDirectory()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
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

    private void SelectProcess_Click(object sender, RoutedEventArgs e)
    {
        var processDialog = new ProcessSelectorDialog();
        processDialog.Owner = this;

        if (processDialog.ShowDialog() == true && !string.IsNullOrEmpty(processDialog.SelectedProcessName))
        {
            MonitoredProcessInput.Text = processDialog.SelectedProcessName;
        }
    }
}
