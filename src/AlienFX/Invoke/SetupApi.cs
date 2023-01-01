using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

/// <summary>
/// Class <c>SetupApi</c> expose access to native windows
/// functions contained in <c>setupapi.dll</c>.
/// </summary>
internal static class SetupApi
{
    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SetupDiGetClassDevs(
        ref Guid ClassGuid,
        IntPtr Enumerator,
        IntPtr hwndParent,
        uint Flags
    );

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool SetupDiEnumDeviceInterfaces(
        IntPtr hDevInfo,
        IntPtr devInfo,
        ref Guid interfaceClassGuid,
        uint memberIndex,
        ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
    );

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr hDevInfo,
        ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
        ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
        uint deviceInterfaceDetailDataSize,
        ref uint requiredSize,
        ref SP_DEVINFO_DATA deviceInfoData
    );
}

[StructLayout(LayoutKind.Sequential)]
internal struct SP_DEVICE_INTERFACE_DATA
{
    public int cbSize;
    public Guid interfaceClassGuid;
    public int flags;
    private UIntPtr reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SP_DEVINFO_DATA
{
    public int cbSize;
    public Guid ClassGuid;
    public uint DevInst;
    public IntPtr Reserved;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
{
    public int cbSize;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string DevicePath;
}
