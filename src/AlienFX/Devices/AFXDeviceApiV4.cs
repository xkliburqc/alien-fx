using AlienFX.Invoke;
using AlienFX.Structures;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;
public sealed class AFXDeviceApiV4 : AFXDevice
{
    private static readonly byte[] s_comm_control = new byte[] { 0x03, 0x21, 0x00, 0x03, 0x00, 0xff };
    private static readonly byte[] s_comm_setOneColor = new byte[] { 0x03, 0x27 };
    private static readonly byte[] s_comm_prepareTurn = new byte[] { 0x03, 0x20, 0x2 };
    private static readonly byte[] s_comm_turnOn = new byte[] { 0x03, 0x26 };

    public AFXDeviceApiV4(SafeFileHandle handle, short length, byte reportId, string devicePath, int vid, int pid)
        : base(handle, length, 4, reportId, devicePath, vid, pid)
    {
    }

    public override bool SetBrightness(byte brightness, bool power)
    {
        uint bright = ((uint)brightness * 0x64) / 0xff;

        if (_isReady)
        {
            UpdateColors();
        }

        base.PrepareAndSend(s_comm_prepareTurn, null);
        //FIXME: Temp code below
        AFXCommand[] mods = new AFXCommand[7];
        mods[0] = new AFXCommand { index = 3, value = (byte)(0x64 - bright) };//Brightness
        mods[1] = new AFXCommand { index = 6, value = 0 };//Light 1
        mods[2] = new AFXCommand { index = 7, value = 1 };//Light 2
        mods[3] = new AFXCommand { index = 8, value = 2 };//Light 3
        mods[4] = new AFXCommand { index = 9, value = 3 };//Light 4
        mods[5] = new AFXCommand { index = 10, value = 4 };//Light 5
        mods[6] = new AFXCommand { index = 5, value = 5 };//Light size
        //END Temp code
        return base.PrepareAndSend(s_comm_turnOn, mods);
    }

    protected override byte GetStatus(byte[] buffer)
    {
        short written = 0;

        if (Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_INPUT_REPORT, 0, 0, buffer, _length, ref written, IntPtr.Zero))
            return buffer[2];

        return 0;
    }

    protected override void Loop() { }

    protected override bool PrepareAndSend(byte[] buffer)
    {
        short written = 0;

        bool res = Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_SET_OUTPUT_REPORT, buffer, _length, 0, 0, ref written, IntPtr.Zero);
        res &= Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_INPUT_REPORT, 0, 0, buffer, _length, ref written, IntPtr.Zero);

        return res;
    }

    protected override bool Reset()
    {
        bool result = PrepareAndSend(s_comm_control, new AFXCommand[] { new AFXCommand { index = 4, value = 4 } });
        result &= PrepareAndSend(s_comm_control, new AFXCommand[] { new AFXCommand { index = 4, value = 1 } });

        _isReady = true;

        return result;
    }

    protected override bool SetColor(uint index, RGBColor color)
    {
        AFXCommand[] mods = new AFXCommand[]
        {
            new AFXCommand { index = 3, value = color.red },
            new AFXCommand { index = 4, value = color.green },
            new AFXCommand { index = 5, value = color.blue },
            new AFXCommand { index = 7, value = 1 },
            new AFXCommand { index = 8, value = (byte)index },
        };

        return PrepareAndSend(s_comm_setOneColor, mods);
    }

    protected override bool UpdateColors()
    {
        bool res = PrepareAndSend(s_comm_control, null);

        if (res)
        {
            _isReady = false;
        }

        return res;
    }
}
