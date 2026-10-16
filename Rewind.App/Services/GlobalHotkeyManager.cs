using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Rewind.App.Services
{
    public class GlobalHotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_NOREPEAT = 0x4000;

        private const int HOTKEY_ID = 9000;
        private IntPtr _windowHandle;
        private HwndSource? _source;

        public event Action? HotkeyPressed;

        public bool Register(IntPtr windowHandle, ModifierKeys modifiers, Key key)
        {
            _windowHandle = windowHandle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(HwndHook);

            uint mod = MOD_NOREPEAT;
            if ((modifiers & ModifierKeys.Control) != 0) mod |= MOD_CONTROL;
            if ((modifiers & ModifierKeys.Shift) != 0) mod |= MOD_SHIFT;
            if ((modifiers & ModifierKeys.Alt) != 0) mod |= MOD_ALT;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            return RegisterHotKey(_windowHandle, HOTKEY_ID, mod, vk);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Unregister()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _source?.RemoveHook(HwndHook);
                _windowHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Unregister();
        }
    }
}
