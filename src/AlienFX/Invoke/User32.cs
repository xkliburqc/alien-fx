﻿using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

public static class User32
{
    public const int HOTKEY_ID = 9000;

    public const uint MOD_NONE = 0x0000;
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
                                        
    public const uint VK_CAPITAL = 0x14;

    public const uint VK_F18 = 0x81;

    [DllImport("user32.dll")]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
