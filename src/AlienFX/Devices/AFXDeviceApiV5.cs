using AlienFX.Invoke;
using AlienFX.Structures;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;
public sealed class AFXDeviceApiV5 : AFXDevice
{
    private static readonly byte[] s_comm_status = new byte[] { 0x93 };
    private static readonly byte[] s_comm_colorSet = new byte[] { 0x8c, 0x02 };
    private static readonly byte[] s_comm_loop = new byte[] { 0x8c, 0x13 };
    private static readonly byte[] s_comm_reset = new byte[] { 0x94 };

    public AFXDeviceApiV5(SafeFileHandle handle, short length, byte reportId)
        : base(handle, length, 5, reportId)
    {
    }

    protected override byte GetStatus(byte[] buffer)
    {
        short written = 0;

        base.PrepareAndSend(s_comm_status, null);
        buffer[1] = 0x93;
        if (Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_FEATURE, 0, 0, buffer, _length, ref written, IntPtr.Zero))
            return buffer[2];

        return 0;
    }

    protected override void Loop() => base.PrepareAndSend(s_comm_loop, null);

    protected override bool PrepareAndSend(byte[] buffer)
    {
        short written = 0;

        return Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_SET_FEATURE, buffer, _length, 0, 0, ref written, IntPtr.Zero);
    }

    protected override bool Reset()
    {
        bool result = PrepareAndSend(s_comm_reset);
        base.GetStatus();

        _isReady = true;

        return result;
    }

    protected override bool SetColor(uint index, RGBColor color)
    {
        AFXCommand[] mods = new AFXCommand[]
        {
            new AFXCommand { index = 4, value = (byte)(index + 1) },
            new AFXCommand { index = 5, value = color.red },
            new AFXCommand { index = 6, value = color.green },
            new AFXCommand { index = 7, value = color.blue },
        };

        return PrepareAndSend(s_comm_colorSet, mods);
    }
}
