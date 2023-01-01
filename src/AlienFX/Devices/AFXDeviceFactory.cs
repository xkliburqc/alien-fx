using System.Runtime.InteropServices;
using AlienFX.Invoke;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;

/// <summary>
/// Class <c>AFXDeviceFactory</c> models factory to
/// retrieve an <see cref="AFXDevice"/>.
/// </summary>
public class AFXDeviceFactory
{
    internal static AFXDevice? GetDevice(string devPath, int vid = -1, int pid = -1)
    {
        AFXDevice? afxDevice = null;
        SafeFileHandle handle;

        if ((handle = Kernel32.CreateFileA(
            devPath,
            FileAccess.Read | FileAccess.Write,
            FileShare.Read | FileShare.Write,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.ReadOnly,
            IntPtr.Zero)) != null)
        {
            IntPtr prep_caps = new();
            HIDP_CAPS caps = new();
            HIDD_ATTRIBUTES attributes = new();
            attributes.Size = Marshal.SizeOf(attributes);

            if (Hid.HidD_GetAttributes(handle, ref attributes) &&
            (vid == -1 || attributes.VendorID == vid) && (pid == -1 || attributes.ProductID == pid))
            {
                Hid.HidD_GetPreparsedData(handle, ref prep_caps);
                _ = Hid.HidP_GetCaps(prep_caps, ref caps);
                Hid.HidD_FreePreparsedData(prep_caps);
                short length = caps.OutputReportByteLength;

                afxDevice = GetDevice(handle, length, caps, attributes.VendorID, devPath, attributes.ProductID);
            }
        }
        return afxDevice;
    }
    
    private static AFXDevice? GetDevice(SafeFileHandle handle, short length, HIDP_CAPS caps, short vendorId, string devicePath, short productId)
    {
        switch (length)
        {
            case 0:
                {
                    if (caps.Usage == 0xcc && vendorId == 0x0d62)
                    {
                        return new AFXDeviceApiV5(handle, caps.FeatureReportByteLength, devicePath, vendorId, productId);
                    }
                }
                break;
            case 34:
                {
                    if (vendorId == 0x187c)
                    {
                        return new AFXDeviceApiV4(handle, length, devicePath, vendorId, productId);
                    }
                }
                break;
        }

        return null;
    }
}
