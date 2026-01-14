using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SimLock.Admin;

public partial class ProcessSelectorDialog : Window
{
    public string? SelectedProcessName { get; private set; }

    private List<ProcessInfo> _allProcesses = new();

    public ProcessSelectorDialog()
    {
        InitializeComponent();
        LoadProcesses();
    }

    private void LoadProcesses()
    {
        try
        {
            _allProcesses = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .GroupBy(p => p.ProcessName)
                .Select(g => new ProcessInfo
                {
                    Name = g.Key,
                    Title = GetMainWindowTitle(g.First())
                })
                .OrderBy(p => p.Name)
                .ToList();

            FilterProcesses();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading processes: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GetMainWindowTitle(Process p)
    {
        try
        {
            return p.MainWindowTitle ?? "";
        }
        catch
        {
            return "";
        }
    }

    private void FilterProcesses()
    {
        var filter = SearchBox.Text?.ToLower() ?? "";

        var filtered = string.IsNullOrEmpty(filter)
            ? _allProcesses
            : _allProcesses.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                p.Title.ToLower().Contains(filter)).ToList();

        ProcessList.ItemsSource = filtered;
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        FilterProcesses();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadProcesses();
    }

    private void ProcessList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SelectButton.IsEnabled = ProcessList.SelectedItem != null;
    }

    private void ProcessList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ProcessList.SelectedItem is ProcessInfo selected)
        {
            SelectedProcessName = selected.Name;
            DialogResult = true;
            Close();
        }
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessList.SelectedItem is ProcessInfo selected)
        {
            SelectedProcessName = selected.Name;
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

public class ProcessInfo
{
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
}
