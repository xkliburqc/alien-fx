using AlienFX.Devices;
using AlienFX.Invoke;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AlienFX;

/// <summary>
/// <code>AFX</code> class.
/// </summary>
public static class AFX
{
    private static readonly nint s_invalid_handle_value = -1;
    private static readonly uint s_buffer_size = 4096;
    private static readonly string s_alienfx_readable_name = "AlienFX";

    /// <summary>
    /// Get a list of Alienware devices supporting AlienFX.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get the device type based on the api version used by the device.
    /// </summary>
    /// <param name="version">The api version associated with the device</param>
    /// <returns>The device type in english</returns>
    public static string GetDeviceType(int version)
    {
        switch (version)
        {
            case 0: return "Desktop";
            case 1: case 2: case 3: return "Notebook";
            case 4: return "Desktop/Notebook";
            case 5: case 8: return "Keyboard";
            case 6: return "Display";
            case 7: return "Mouse";
        }
        return "Unknown";
    }

    /// <summary>
    /// Register a new hotkey to a window including modifiers.
    /// </summary>
    /// <param name="hWnd">A window handle</param>
    /// <param name="id"></param>
    /// <param name="fsModifiers"></param>
    /// <param name="vk"></param>
    /// <returns></returns>
    public static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk)
    {
        return User32.RegisterHotKey(hWnd, id, fsModifiers, vk);
    }

    /// <summary>
    /// Unregister a hotkey previously registered with <code>RegisterHotkey</code>
    /// </summary>
    /// <param name="hWnd">A window handle</param>
    /// <param name="id">An <code>id</code> previously used in <code>RegisterHotKey</code></param>
    /// <returns>True, if the hotkey is successfully unregistered</returns>
    public static bool UnregisterHotKey(IntPtr hWnd, int id)
    {
        return User32.UnregisterHotKey(hWnd, id);
    }

    /// <summary>
    /// Get the power and battery status details.
    /// </summary>
    /// <returns></returns>
    public static SystemPowerStatus GetSystemPowerStatus()
    {
        _ = Kernel32.GetSystemPowerStatus(out SystemPowerStatus sps);
        return sps;
    }

    public static bool GetAlienFXPowerSetting()
    {
        Dictionary<string, string> powerProfile = PowerProfile.GetCurrentPowerProfile();

        if (powerProfile.ContainsKey(s_alienfx_readable_name))
        {
            string value = powerProfile[s_alienfx_readable_name];
            return value == "1" ? true : false;
        }

        return true;
    }
}
