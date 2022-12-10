using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace AlienFX.Invoke;

internal static class Hid
{
    [DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
    internal static extern void HidD_GetHidGuid(out Guid hidGuid);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

    [DllImport("hid.dll", SetLastError = true)]
    internal static extern bool HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    internal static extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

    [DllImport("hid.dll", SetLastError = true)]
    internal static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);
}

[StructLayout(LayoutKind.Sequential)]
internal struct HIDD_ATTRIBUTES
{
    public int Size;
    public short VendorID;
    public short ProductID;
    public short VersionNumber;
}

internal struct HIDP_CAPS
{
    public short Usage;
    public short UsagePage;
    public short InputReportByteLength;
    public short OutputReportByteLength;
    public short FeatureReportByteLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
    public short[] Reserved;
    public short NumberLinkCollectionNodes;
    public short NumberInputButtonCaps;
    public short NumberInputValueCaps;
    public short NumberInputDataIndices;
    public short NumberOutputButtonCaps;
    public short NumberOutputValueCaps;
    public short NumberOutputDataIndices;
    public short NumberFeatureButtonCaps;
    public short NumberFeatureValueCaps;
    public short NumberFeatureDataIndices;
}
