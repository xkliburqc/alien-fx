using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

/// <summary>
/// Class <c>User32</c> expose access to native windows
/// functions contained in <c>user32.dll</c>.
/// </summary>
public static class User32
{
    /// <summary>Represents the hotkey id.</summary>
    public const int HOTKEY_ID = 9000;
    /// <summary>Represents the hotkey modifier NONE.</summary>
    public const uint MOD_NONE = 0x0000;
    /// <summary>Represents the hotkey modifier ALT.</summary>
    public const uint MOD_ALT = 0x0001;
    /// <summary>Represents the hotkey modifier CONTROL.</summary>
    public const uint MOD_CONTROL = 0x0002;
    /// <summary>Represents the hotkey modifier SHIFT.</summary>
    public const uint MOD_SHIFT = 0x0004;
    /// <summary>Represents the hotkey modifier WIN.</summary>
    public const uint MOD_WIN = 0x0008;
    /// <summary>Represents the virtual key for shift.</summary>
    public const uint VK_CAPITAL = 0x14;
    /// <summary>Represents the virtual key for F18.</summary>
    public const uint VK_F18 = 0x81;

    [DllImport("user32.dll")]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
