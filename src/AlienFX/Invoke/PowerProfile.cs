using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AlienFX.Invoke;

/// <summary>
/// Class <c>PowerProfile</c> expose access to native windows
/// functions contained in <c>powrprof.dll</c>.
/// </summary>
internal class PowerProfile
{
    public static readonly uint s_error_more_data = 234;

    public static Guid NO_SUBGROUP_GUID = new Guid("fea3413e-7e05-4911-9a71-700331f1c294");
    public static readonly Guid GUID_DISK_SUBGROUP = new Guid("0012ee47-9041-4b5d-9b77-535fba8b1442");
    public static readonly Guid GUID_SYSTEM_BUTTON_SUBGROUP = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
    public static readonly Guid GUID_PROCESSOR_SETTINGS_SUBGROUP = new Guid("54533251-82be-4824-96c1-47b60b740d00");
    public static Guid GUID_VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
    public static  Guid GUID_BATTERY_SUBGROUP = new Guid("e73a048d-bf27-4f12-9731-8b2076e8891f");
    public static readonly Guid GUID_SLEEP_SUBGROUP = new Guid("238C9FA8-0AAD-41ED-83F4-97BE242C8F20");
    public static readonly Guid GUID_PCIEXPRESS_SETTINGS_SUBGROUP = new Guid("501a4d13-42af-4429-9fd1-a8218c268e20");

    [DllImport("powrprof.dll")]
    internal static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);

    [DllImport("powrprof.dll", CharSet = CharSet.Unicode)]
    internal static extern uint PowerReadFriendlyName(
        IntPtr RootPowerKey,
        IntPtr SchemeGuid,
        IntPtr SubGroupOfPowerSettingGuid,
        IntPtr PowerSettingGuid,
        StringBuilder Buffer,
        ref uint BufferSize);

    [DllImport("powrprof.dll")]
    internal static extern uint PowerEnumerate(
        IntPtr RootPowerKey,
        IntPtr SchemeGuid,
        ref Guid SubGroupOfPowerSetting,
        uint AccessFlags,
        uint Index,
        ref Guid Buffer,
        ref uint BufferSize);

    [DllImport("powrprof.dll")]
    internal static extern uint PowerReadDCValue(
        IntPtr RootPowerKey,
        IntPtr SchemeGuid,
        IntPtr SubGroupOfPowerSettingGuid,
        ref Guid PowerSettingGuid,
        ref int Type,
        ref IntPtr Buffer,
        ref uint BufferSize);

    [DllImport("powrprof.dll")]
    internal static extern uint PowerReadACValue(
        IntPtr RootPowerKey,
        IntPtr SchemeGuid,
        IntPtr SubGroupOfPowerSettingGuid,
        ref Guid PowerSettingGuid,
        ref int Type,
        ref IntPtr Buffer,
        ref uint BufferSize);

    public static Dictionary<string, string> GetCurrentPowerProfile()
    {
        Dictionary<string, string> values = new();
        IntPtr activeGuidPtr = IntPtr.Zero;
        try
        {
            _ = Kernel32.GetSystemPowerStatus(out SystemPowerStatus sps);

            uint res = PowerGetActiveScheme(IntPtr.Zero, ref activeGuidPtr);
            if (res != 0)
                throw new Win32Exception();

            //Get Friendly Name
            uint buffSize = 0;
            StringBuilder buffer = new StringBuilder();
            Guid subGroupGuid = Guid.Empty;
            Guid powerSettingGuid = Guid.Empty;
            res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr,
                IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize);

            if (res == s_error_more_data)
            {
                buffer.Capacity = (int)buffSize;
                res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr,
                    IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize);
            }

            if (res != 0)
                throw new Win32Exception();

            //Get the Power Settings
            Guid VideoSettingGuid = Guid.Empty;
            uint index = 0;
            uint BufferSize = Convert.ToUInt32(Marshal.SizeOf(typeof(Guid)));

            while (PowerEnumerate(IntPtr.Zero, activeGuidPtr, ref NO_SUBGROUP_GUID,
                18, index, ref VideoSettingGuid, ref BufferSize) == 0)
            {
                uint size = 4;
                IntPtr temp = IntPtr.Zero;
                int type = 0;

                if (sps.ACLineStatus == ACLineStatus.Online)
                {
                    res = PowerReadACValue(IntPtr.Zero, activeGuidPtr, IntPtr.Zero,
                    ref VideoSettingGuid, ref type, ref temp, ref size);
                }
                else
                {
                    res = PowerReadDCValue(IntPtr.Zero, activeGuidPtr, IntPtr.Zero,
                    ref VideoSettingGuid, ref type, ref temp, ref size);
                }

                IntPtr pSubGroup = Marshal.AllocHGlobal(Marshal.SizeOf(NO_SUBGROUP_GUID));
                Marshal.StructureToPtr(GUID_VIDEO_SUBGROUP, pSubGroup, false);
                IntPtr pSetting = Marshal.AllocHGlobal(Marshal.SizeOf(VideoSettingGuid));
                Marshal.StructureToPtr(VideoSettingGuid, pSetting, false);

                uint builderSize = 200;
                StringBuilder builder = new((int)builderSize);
                res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr,
                    pSubGroup, pSetting, builder, ref builderSize);
                values.Add(builder.ToString(), temp.ToString());

                index++;
            }
        }
        finally
        {
            if (activeGuidPtr != IntPtr.Zero)
            {
                IntPtr res = Kernel32.LocalFree(activeGuidPtr);
                if (res != IntPtr.Zero)
                    throw new Win32Exception();
            }
        }

        return values;
    }
}
