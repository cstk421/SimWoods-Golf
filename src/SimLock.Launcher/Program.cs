using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SimLock.Launcher
{
    static class Program
    {
        private const string DotNetDownloadUrl = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe";
        private const string RequiredRuntimeVersion = "8.0";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (IsDotNet8DesktopRuntimeInstalled())
            {
                LaunchSimLock();
            }
            else
            {
                PromptToInstallDotNet();
            }
        }

        static bool IsDotNet8DesktopRuntimeInstalled()
        {
            try
            {
                // Check registry for .NET Desktop Runtime 8.x
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App"))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            if (valueName.StartsWith("8."))
                            {
                                return true;
                            }
                        }
                    }
                }

                // Also check x86 path
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App"))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            if (valueName.StartsWith("8."))
                            {
                                return true;
                            }
                        }
                    }
                }

                // Fallback: try running dotnet command
                return CheckDotNetViaCommand();
            }
            catch
            {
                return CheckDotNetViaCommand();
            }
        }

        static bool CheckDotNetViaCommand()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return false;

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return output.Contains("Microsoft.WindowsDesktop.App 8.");
                }
            }
            catch
            {
                return false;
            }
        }

        static void LaunchSimLock()
        {
            try
            {
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var simLockPath = Path.Combine(currentDir, "SimLock.exe");

                if (File.Exists(simLockPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = simLockPath,
                        WorkingDirectory = currentDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "SimLock.exe was not found.\n\nPlease ensure SimLock.exe is in the same folder as this launcher.",
                        "SimLock - Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch SimLock:\n\n{ex.Message}",
                    "SimLock - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        static void PromptToInstallDotNet()
        {
            var result = MessageBox.Show(
                ".NET 8 Desktop Runtime is required to run SimLock.\n\n" +
                "Would you like to download and install it now?\n\n" +
                "After installation, please run this launcher again.",
                "SimLock - Runtime Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = DotNetDownloadUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to open download page:\n\n{ex.Message}\n\n" +
                        "Please visit https://dotnet.microsoft.com/download/dotnet/8.0 to download manually.",
                        "SimLock - Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "SimLock cannot run without .NET 8 Desktop Runtime.\n\n" +
                    "You can download it later from:\nhttps://dotnet.microsoft.com/download/dotnet/8.0",
                    "SimLock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
