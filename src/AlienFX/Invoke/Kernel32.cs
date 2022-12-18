using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

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
        [MarshalAs(UnmanagedType.AsAny)]
        [In] object InBuffer,
        short nInBufferSize,
        [MarshalAs(UnmanagedType.AsAny)]
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

public enum ACLineStatus : byte
{
    Offline = 0,
    Online = 1,
    Unknown = 255
}
public enum BatteryFlag : byte
{
    High = 0x01,
    Low = 0x02,
    Critical = 0x04,
    Charging = 0x08,
    NoSystemBattery = 0x80,
    Unknown = 0xFF
}

public struct SystemPowerStatus
{
    public ACLineStatus ACLineStatus;
    public BatteryFlag BatteryFlag;
    public byte BatteryLifePercent;
    public byte Reserved1;
    public uint BatteryLifeTime;
    public uint BatteryFullLifeTime;
}
