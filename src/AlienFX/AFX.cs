using AlienFX.Devices;
using AlienFX.Invoke;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace AlienFX;

public static class AFX
{
    private static readonly nint s_invalid_handle_value = -1;
    private static readonly uint s_buffer_size = 4096;

    public static List<AFXDevice> FindDevices()
    {
        List<AFXDevice> devices = new();

        Hid.HidD_GetHidGuid(out Guid hidGuid);
        IntPtr handle = SetupApi.SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, 0x00000002 | 0x00000010);

        if (handle != s_invalid_handle_value)
        {
            bool success = true;
            uint i = 0;

            while (success)
            {
                SP_DEVICE_INTERFACE_DATA dia = new();
                dia.cbSize = Marshal.SizeOf(dia);

                success = SetupApi.SetupDiEnumDeviceInterfaces(handle, IntPtr.Zero, ref hidGuid, i, ref dia);
                if (success)
                {
                    SP_DEVINFO_DATA da = new();
                    da.cbSize = Marshal.SizeOf(da);

                    SP_DEVICE_INTERFACE_DETAIL_DATA didd = new()
                    {
                        cbSize = IntPtr.Size == 8 ? 8 : 5
                    };

                    uint nRequiredSize = 0;
                    uint nBytes = s_buffer_size;

                    if (SetupApi.SetupDiGetDeviceInterfaceDetail(handle, ref dia, ref didd, nBytes, ref nRequiredSize, ref da))
                    {
                        AFXDevice? device = AFXDeviceFactory.GetDevice(didd.DevicePath);
                        if (device != null) 
                        {
                            devices.Add(device);
                        }
                    }

                    i++;
                }
            }
        }

        return devices;
    }
}
