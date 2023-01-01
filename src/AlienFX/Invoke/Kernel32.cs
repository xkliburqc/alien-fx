using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

/// <summary>
/// Class <c>Kernel32</c> expose access to native windows
/// functions contained in <c>kernel32.dll</c>.
/// </summary>
internal static class Kernel32
{
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern SafeFileHandle CreateFileA(
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        [MarshalAs(UnmanagedType.U4)] FileAccess access,
        [MarshalAs(UnmanagedType.U4)] FileShare share,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr hHandle);

    [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        EIOControlCode IoControlCode,
#pragma warning disable CS0618 // Type or member is obsolete
        [MarshalAs(UnmanagedType.AsAny)]
#pragma warning restore CS0618 // Type or member is obsolete
        [In] object InBuffer,
        short nInBufferSize,
#pragma warning disable CS0618 // Type or member is obsolete
        [MarshalAs(UnmanagedType.AsAny)]
#pragma warning restore CS0618 // Type or member is obsolete
        [Out] object OutBuffer,
        short nOutBufferSize,
        ref short pBytesReturned,
        IntPtr lpOverlapped);

    [DllImport("kernel32")]
    internal static extern bool GetSystemPowerStatus(out SystemPowerStatus sps);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr LocalFree(IntPtr hMem);
}

[Flags]
internal enum EIOControlCode : uint
{
    IOCTL_HID_GET_FEATURE = (((0x0000000b) << 16) | ((0) << 14) | (((100)) << 2) | (2)),
    IOCTL_HID_GET_INPUT_REPORT = (((0x0000000b) << 16) | ((0) << 14) | (((104)) << 2) | (2)),
    IOCTL_HID_SET_FEATURE = (((0x0000000b) << 16) | ((0) << 14) | (((100)) << 2) | (1)),
    IOCTL_HID_SET_OUTPUT_REPORT = (((0x0000000b) << 16) | ((0) << 14) | (((101)) << 2) | (1)),
}

/// <summary>
/// Enum <c>ACLineStatus</c> enumerates options of the AC line status.
/// </summary>
public enum ACLineStatus : byte
{
    /// <summary>Alternating current is offline. Battery is in use.</summary>
    Offline = 0,
    /// <summary>Alternating current is online.</summary>
    Online = 1,
    /// <summary>Alternating current status is unknow.</summary>
    Unknown = 255
}

/// <summary>
/// Enum <c>BatteryFlag</c> enumerates battery level values.
/// </summary>
public enum BatteryFlag : byte
{
    /// <summary>Battery level is high.</summary>
    High = 0x01,
    /// <summary>Battery level is low.</summary>
    Low = 0x02,
    /// <summary>Battery level is critical.</summary>
    Critical = 0x04,
    /// <summary>Battery is charging.</summary>
    Charging = 0x08,
    /// <summary>No battery in the system status.</summary>
    NoSystemBattery = 0x80,
    /// <summary>Unknow battery status.</summary>
    Unknown = 0xFF
}

/// <summary>
/// Struct <c>SystemPowerStatus</c> models the system power/battery status.
/// </summary>
public struct SystemPowerStatus
{
    /// <summary>Gets the status of the system power/battery.</summary>
    public ACLineStatus ACLineStatus;
    /// <summary>Gets the battery level of the system.</summary>
    public BatteryFlag BatteryFlag;
    /// <summary>Gets the percentage of the battery life.</summary>
    public byte BatteryLifePercent;
    /// <summary>Reserved field for methods using this structure.</summary>
    public byte Reserved1;
    /// <summary>Gets the remaining time of the battery.</summary>
    public uint BatteryLifeTime;
    /// <summary>Gets the remaining time until the battery is fully charged.</summary>
    public uint BatteryFullLifeTime;
}
