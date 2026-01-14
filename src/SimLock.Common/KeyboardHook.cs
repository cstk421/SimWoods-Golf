using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SimLock.Common;

public class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private static readonly IntPtr HookId = IntPtr.Zero;
    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookHandle = IntPtr.Zero;
    private bool _isEnabled = false;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // Virtual key codes
    private const int VK_TAB = 0x09;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_F4 = 0x73;
    private const int VK_LALT = 0xA4;
    private const int VK_RALT = 0xA5;

    [Flags]
    private enum KeyFlags
    {
        None = 0,
        AltPressed = 0x20
    }

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    public void Enable()
    {
        if (_isEnabled) return;

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;

        if (curModule != null)
        {
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                GetModuleHandle(curModule.ModuleName), 0);
            _isEnabled = true;
        }
    }

    public void Disable()
    {
        if (!_isEnabled) return;

        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
        _isEnabled = false;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            int flags = Marshal.ReadInt32(lParam + 8);
            bool altPressed = (flags & (int)KeyFlags.AltPressed) != 0;

            // Block Windows keys
            if (vkCode == VK_LWIN || vkCode == VK_RWIN)
            {
                return (IntPtr)1;
            }

            // Block Alt+Tab
            if (vkCode == VK_TAB && altPressed)
            {
                return (IntPtr)1;
            }

            // Block Alt+F4
            if (vkCode == VK_F4 && altPressed)
            {
                return (IntPtr)1;
            }

            // Block Alt+Escape
            if (vkCode == VK_ESCAPE && altPressed)
            {
                return (IntPtr)1;
            }

            // Block Ctrl+Escape
            if (vkCode == VK_ESCAPE && (Control.ModifierKeys & Keys.Control) != 0)
            {
                return (IntPtr)1;
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }
}

// Minimal Keys enum for modifier key detection
internal static class Control
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_CONTROL = 0x11;
    private const int VK_SHIFT = 0x10;
    private const int VK_MENU = 0x12; // Alt key

    public static Keys ModifierKeys
    {
        get
        {
            Keys keys = Keys.None;
            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0) keys |= Keys.Control;
            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) keys |= Keys.Shift;
            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0) keys |= Keys.Alt;
            return keys;
        }
    }
}

[Flags]
internal enum Keys
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4
}
