using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SimLock.Common;

namespace SimLock.Locker;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private readonly KeyboardHook _keyboardHook;
    private readonly DispatcherTimer _videoTimer;
    private string _currentPin = "";
    private bool _isNewUser = false;
    private TimeSpan _videoDuration;

    public MainWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load();
        _keyboardHook = new KeyboardHook();

        _videoTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _videoTimer.Tick += VideoTimer_Tick;

        // Apply theme from config
        ThemeManager.ApplyTheme(Application.Current, _config);

        LoadAssets();
        ConfigureButtons();
    }

    private void LoadAssets()
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

                SplashLogo.Source = bitmap;
                MenuLogo.Source = bitmap;
                PinLogo.Source = bitmap;
                VideoLogo.Source = bitmap;
            }

            // Splash screen text
            SplashTitleText.Text = _config.SplashTitle;
            SplashSubtitleText.Text = _config.SplashSubtitle;

            // PIN screen title
            PinScreenTitleText.Text = _config.PinScreenTitle;

            // Custom message on all screens
            if (_config.ShowCustomMessage && !string.IsNullOrWhiteSpace(_config.CustomMessage))
            {
                SplashCustomMessage.Text = _config.CustomMessage;
                SplashCustomMessage.Visibility = Visibility.Visible;

                MenuCustomMessage.Text = _config.CustomMessage;
                MenuCustomMessage.Visibility = Visibility.Visible;

                PinCustomMessage.Text = _config.CustomMessage;
                PinCustomMessage.Visibility = Visibility.Visible;
            }

            // Load splash background image if configured
            LoadSplashBackground();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading assets: {ex.Message}");
        }
    }

    private void LoadSplashBackground()
    {
        try
        {
            if (_config.UseSplashBackgroundImage && !string.IsNullOrWhiteSpace(_config.SplashBackgroundImagePath))
            {
                var bgPath = _config.SplashBackgroundImagePath;
                if (!Path.IsPathRooted(bgPath))
                {
                    bgPath = Path.Combine(AppConfig.GetDataDirectory(), bgPath);
                }

                if (File.Exists(bgPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(bgPath, UriKind.Absolute);
                    bitmap.EndInit();

                    SplashBackgroundImage.Source = bitmap;
                    SplashBackgroundImage.Visibility = Visibility.Visible;

                    // Apply text box opacity
                    var opacity = Math.Clamp(_config.SplashTextBoxOpacity, 0.0, 1.0);
                    var hexOpacity = ((int)(opacity * 255)).ToString("X2");
                    var bgColor = _config.ThemeSurfaceColor.TrimStart('#');
                    if (bgColor.Length == 6)
                    {
                        SplashTextOverlay.Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString($"#{hexOpacity}{bgColor}"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading splash background: {ex.Message}");
        }
    }

    private void ConfigureButtons()
    {
        // Configure button visibility
        ReturningGolferButton.Visibility = _config.ShowReturningGolferButton
            ? Visibility.Visible
            : Visibility.Collapsed;

        TutorialButton.Visibility = _config.ShowTutorialButton
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Custom Button 1
        if (_config.ShowCustomButton1)
        {
            CustomButton1.Content = _config.CustomButton1Label;
            CustomButton1.Visibility = Visibility.Visible;
        }
        else
        {
            CustomButton1.Visibility = Visibility.Collapsed;
        }

        // Custom Button 2
        if (_config.ShowCustomButton2)
        {
            CustomButton2.Content = _config.CustomButton2Label;
            CustomButton2.Visibility = Visibility.Visible;
        }
        else
        {
            CustomButton2.Visibility = Visibility.Collapsed;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Enable keyboard hook to block system keys
        _keyboardHook.Enable();

        // Set focus to window
        this.Focus();

        // Check activation status
        if (!_config.IsActivated)
        {
            ShowActivationRequired();
            return;
        }

        // Normal startup - show splash screen
        HideAllScreens();
        SplashScreen.Visibility = Visibility.Visible;
    }

    private void ShowActivationRequired()
    {
        HideAllScreens();
        ActivationRequiredScreen.Visibility = Visibility.Visible;
    }

    private void CloseActivationRequired_Click(object sender, RoutedEventArgs e)
    {
        // Close the application when activation is required but user clicks close
        _keyboardHook.Disable();
        Application.Current.Shutdown();
    }

    private void SplashScreen_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ShowMainMenu();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // Block input on activation required screen
        if (ActivationRequiredScreen.Visibility == Visibility.Visible)
        {
            e.Handled = true;
            return;
        }

        // Handle PIN entry keyboard input
        if (PinEntryScreen.Visibility == Visibility.Visible)
        {
            HandlePinKeyboardInput(e);
            return;
        }

        // Handle splash screen - any key advances to main menu
        if (SplashScreen.Visibility == Visibility.Visible)
        {
            ShowMainMenu();
            e.Handled = true;
            return;
        }

        // Block escape key on other screens
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
        }
    }

    private void HandlePinKeyboardInput(KeyEventArgs e)
    {
        // Number keys (main keyboard)
        if (e.Key >= Key.D0 && e.Key <= Key.D9)
        {
            var digit = (e.Key - Key.D0).ToString();
            AddPinDigit(digit);
            e.Handled = true;
            return;
        }

        // Number keys (numpad)
        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            var digit = (e.Key - Key.NumPad0).ToString();
            AddPinDigit(digit);
            e.Handled = true;
            return;
        }

        // Backspace - delete last digit
        if (e.Key == Key.Back)
        {
            if (_currentPin.Length > 0)
            {
                _currentPin = _currentPin[..^1];
                UpdatePinDisplay();
                PinErrorText.Visibility = Visibility.Collapsed;
            }
            e.Handled = true;
            return;
        }

        // Delete or Escape - clear all digits
        if (e.Key == Key.Delete || e.Key == Key.Escape)
        {
            _currentPin = "";
            UpdatePinDisplay();
            PinErrorText.Visibility = Visibility.Collapsed;
            e.Handled = true;
            return;
        }

        // Enter - validate if 4 digits entered
        if (e.Key == Key.Enter && _currentPin.Length == 4)
        {
            ValidatePin();
            e.Handled = true;
            return;
        }
    }

    private void AddPinDigit(string digit)
    {
        if (_currentPin.Length >= 4) return;

        _currentPin += digit;
        UpdatePinDisplay();
        PinErrorText.Visibility = Visibility.Collapsed;

        if (_currentPin.Length == 4)
        {
            ValidatePin();
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        // If on splash screen, go to main menu
        if (SplashScreen.Visibility == Visibility.Visible)
        {
            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        HideAllScreens();
        MainMenuScreen.Visibility = Visibility.Visible;
    }

    private void ShowPinEntry()
    {
        HideAllScreens();
        _currentPin = "";
        _isNewUser = false;
        UpdatePinDisplay();
        PinErrorText.Visibility = Visibility.Collapsed;
        PinEntryScreen.Visibility = Visibility.Visible;
    }

    private void ShowVideoPlayer()
    {
        HideAllScreens();
        VideoPlayerScreen.Visibility = Visibility.Visible;
        _isNewUser = true;
        LoadAndPlayVideo();
    }

    private void ShowVideoPlayer(string videoPath)
    {
        HideAllScreens();
        VideoPlayerScreen.Visibility = Visibility.Visible;
        _isNewUser = false;
        LoadAndPlayVideo(videoPath);
    }

    private void HideAllScreens()
    {
        SplashScreen.Visibility = Visibility.Collapsed;
        MainMenuScreen.Visibility = Visibility.Collapsed;
        PinEntryScreen.Visibility = Visibility.Collapsed;
        VideoPlayerScreen.Visibility = Visibility.Collapsed;
        ActivationRequiredScreen.Visibility = Visibility.Collapsed;

        // Stop video if playing
        StopVideo();
    }

    private void ReturningGolfer_Click(object sender, RoutedEventArgs e)
    {
        ShowPinEntry();
    }

    private void NewGolfer_Click(object sender, RoutedEventArgs e)
    {
        ShowVideoPlayer();
    }

    private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
    {
        ShowMainMenu();
    }

    #region Custom Button Handlers

    private void CustomButton1_Click(object sender, RoutedEventArgs e)
    {
        ExecuteCustomAction(_config.CustomButton1ActionType, _config.CustomButton1Target);
    }

    private void CustomButton2_Click(object sender, RoutedEventArgs e)
    {
        ExecuteCustomAction(_config.CustomButton2ActionType, _config.CustomButton2Target);
    }

    private void ExecuteCustomAction(string actionType, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            MessageBox.Show(
                "No target configured for this action.",
                "Configuration Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            switch (actionType)
            {
                case "RunExecutable":
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                    UnlockAndClose();
                    break;

                case "OpenUrl":
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                    UnlockAndClose();
                    break;

                case "PlayLocalVideo":
                    if (!File.Exists(target))
                    {
                        MessageBox.Show(
                            $"Video file not found: {target}",
                            "File Not Found",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    ShowVideoPlayer(target);
                    break;

                default:
                    MessageBox.Show(
                        $"Unknown action type: {actionType}",
                        "Configuration Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error executing action: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion

    #region PIN Entry

    private void NumPad_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPin.Length >= 4) return;

        var button = (Button)sender;
        _currentPin += button.Content.ToString();
        UpdatePinDisplay();

        if (_currentPin.Length == 4)
        {
            ValidatePin();
        }
    }

    private void ClearPin_Click(object sender, RoutedEventArgs e)
    {
        _currentPin = "";
        UpdatePinDisplay();
        PinErrorText.Visibility = Visibility.Collapsed;
    }

    private void DeletePin_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPin.Length > 0)
        {
            _currentPin = _currentPin[..^1];
            UpdatePinDisplay();
            PinErrorText.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdatePinDisplay()
    {
        Pin1.Text = _currentPin.Length >= 1 ? "*" : "";
        Pin2.Text = _currentPin.Length >= 2 ? "*" : "";
        Pin3.Text = _currentPin.Length >= 3 ? "*" : "";
        Pin4.Text = _currentPin.Length >= 4 ? "*" : "";
    }

    private void ValidatePin()
    {
        if (_currentPin == _config.UnlockCode)
        {
            UnlockAndClose();
        }
        else
        {
            PinErrorText.Visibility = Visibility.Visible;
            _currentPin = "";
            UpdatePinDisplay();
        }
    }

    #endregion

    #region Video Player

    private void LoadAndPlayVideo()
    {
        LoadAndPlayVideo(_config.GetAbsoluteVideoPath());
    }

    private void LoadAndPlayVideo(string videoPath)
    {
        try
        {
            if (!File.Exists(videoPath))
            {
                MessageBox.Show(
                    "Video not found. Please contact an administrator.",
                    "Video Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                ShowMainMenu();
                return;
            }

            TimeRemainingText.Text = "Loading video...";
            VideoPlayer.Source = new Uri(videoPath, UriKind.Absolute);
            VideoPlayer.Volume = 1.0;
            VideoPlayer.Play();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading video: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            ShowMainMenu();
        }
    }

    private void StopVideo()
    {
        _videoTimer.Stop();
        VideoPlayer.Stop();
        VideoPlayer.Source = null;
    }

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            _videoDuration = VideoPlayer.NaturalDuration.TimeSpan;
            UpdateTimeRemaining();
            _videoTimer.Start();
        }
    }

    private void VideoTimer_Tick(object? sender, EventArgs e)
    {
        UpdateTimeRemaining();
    }

    private void UpdateTimeRemaining()
    {
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            var remaining = _videoDuration - VideoPlayer.Position;
            if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;
            TimeRemainingText.Text = $"Time Remaining: {FormatTime(remaining)}";
        }
    }

    private static string FormatTime(TimeSpan time)
    {
        if (time.Hours > 0)
            return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}";
        return $"{time.Minutes}:{time.Seconds:D2}";
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _videoTimer.Stop();
        // Auto-unlock after video completes (only for tutorial video)
        if (_isNewUser)
        {
            UnlockAndClose();
        }
        else
        {
            // Custom video - return to main menu
            ShowMainMenu();
        }
    }

    private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        _videoTimer.Stop();
        MessageBox.Show(
            $"Error playing video: {e.ErrorException?.Message ?? "Unknown error"}\n\nPlease contact an administrator.",
            "Video Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        ShowMainMenu();
    }

    #endregion

    private void UnlockAndClose()
    {
        _keyboardHook.Disable();
        this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _videoTimer.Stop();
        _keyboardHook.Dispose();
        base.OnClosed(e);
    }
}
