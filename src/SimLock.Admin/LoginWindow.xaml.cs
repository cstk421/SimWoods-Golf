using System.Windows;
using SimLock.Common;

namespace SimLock.Admin;

public partial class LoginWindow : Window
{
    private readonly AppConfig _config;

    public LoginWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load();
        PasswordInput.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        AttemptLogin();
    }

    private void PasswordInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            AttemptLogin();
        }
    }

    private void AttemptLogin()
    {
        if (PasswordInput.Password == _config.AdminPassword)
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
            this.Close();
        }
        else
        {
            ErrorText.Visibility = Visibility.Visible;
            PasswordInput.Clear();
            PasswordInput.Focus();
        }
    }
}
