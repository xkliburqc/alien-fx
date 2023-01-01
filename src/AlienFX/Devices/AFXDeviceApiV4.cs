using System.Drawing;
using AlienFX.Invoke;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;

/// <summary>
/// Class <c>AFXDeviceApiV4</c> models a device that support AlienFX API version 4.
/// <remarks>
/// An <c>AFXDeviceApiV4</c> implement device specific logic.
/// A device should be created using the <see cref="AFXDeviceFactory"/> factory.
/// </remarks>
/// </summary>
public sealed class AFXDeviceApiV4 : AFXDevice
{
    private static readonly byte[] s_comm_control = new byte[] { 0x03, 0x21, 0x00, 0x03, 0x00, 0xff };
    private static readonly byte[] s_comm_setOneColor = new byte[] { 0x03, 0x27 };
    private static readonly byte[] s_comm_prepareTurn = new byte[] { 0x03, 0x20, 0x2 };
    private static readonly byte[] s_comm_turnOn = new byte[] { 0x03, 0x26 };

    /// <summary>
    /// This constructor initializes the new Device to
    /// (<paramref name="handle"/>,
    /// <paramref name="length"/>,
    /// <paramref name="devicePath"/>,
    /// <paramref name="vid"/>,
    /// <paramref name="pid"/>,
    /// <remarks>
    /// A device should be initialized using the <see cref="AFXDeviceFactory"/> factory.
    /// This constructor default the api version and report id.
    /// </remarks>
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="length">The device buffer size.</param>
    /// <param name="devicePath">The hardware path of a device.</param>
    /// <param name="vid">The device's vendor id.</param>
    /// <param name="pid">The device's product id.</param>
    internal AFXDeviceApiV4(SafeFileHandle handle, short length, string devicePath, int vid, int pid)
        : base(handle, length, devicePath, vid, pid, 4, 0)
    {
    }

    /// <summary>
    /// This method set the brightness to all the device's lights.
    /// </summary>
    /// <param name="brightness">A <c>byte</c> value for the brightness.</param>
    /// <returns>True, if the brightness was updated properly.</returns>
    public override bool SetBrightness(byte brightness)
    {
        uint bright = ((uint)brightness * 0x64) / 0xff;

        if (_isReady)
        {
            UpdateColors();
        }

        base.PrepareAndSend(s_comm_prepareTurn, null);

        //FIXME: Temp code below
        KeyValuePair<byte, byte>[] mods = new KeyValuePair<byte, byte>[7];
        mods[0] = new KeyValuePair<byte, byte>(3, (byte)(0x64 - bright));//Brightness
        mods[1] = new KeyValuePair<byte, byte>(6, 0);//Light 1
        mods[2] = new KeyValuePair<byte, byte>(7, 1);//Light 2
        mods[3] = new KeyValuePair<byte, byte>(8, 2);//Light 3
        mods[4] = new KeyValuePair<byte, byte>(9, 3);//Light 4
        mods[5] = new KeyValuePair<byte, byte>(10, 4);//Light 5
        mods[6] = new KeyValuePair<byte, byte>(5, 5);//Light size
        //END Temp code

        bool success = PrepareAndSend(s_comm_turnOn, mods);

        return success;
    }

    /// <summary>
    /// This method get the AlienFX device status. The buffer is used also to retrieve results.
    /// </summary>
    /// <param name="buffer">A buffer containing the device report id.</param>
    /// <returns>The device status value.</returns>
    protected override byte GetStatus(byte[] buffer)
    {
        short written = 0;

        if (Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_INPUT_REPORT, 0, 0, buffer, _length, ref written, IntPtr.Zero))
            return buffer[2];

        return 0;
    }

    /// <summary>
    /// This method is used to send a loop command through the api
    /// but is not used in api version 4.
    /// </summary>
    protected override void Loop() { }

    /// <summary>
    /// This method send and AlienFX command with optional modifiers
    /// through the AlienFX api. The command and modifiers are
    /// bundled in a buffer.
    /// </summary>
    /// <param name="buffer">A <c>byte array</c> containing the command and optional modifiers.</param>
    /// <returns>True, if the command was sent properly.</returns>
    protected override bool SendCommand(byte[] buffer)
    {
        short written = 0;

        bool res = Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_SET_OUTPUT_REPORT, buffer, _length, 0, 0, ref written, IntPtr.Zero);
        res &= Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_INPUT_REPORT, 0, 0, buffer, _length, ref written, IntPtr.Zero);

        return res;
    }

    /// <summary>
    /// This method reset the device status to allow changes.
    /// </summary>
    /// <returns>True, if the device is properly reset.</returns>
    protected override bool Reset()
    {
        bool result = PrepareAndSend(s_comm_control, new KeyValuePair<byte, byte>(4, 4));
        result &= PrepareAndSend(s_comm_control, new KeyValuePair<byte, byte>(4, 1));

        _isReady = true;

        return result;
    }

    /// <summary>
    /// This method change the RGB color of a specific device light.
    /// </summary>
    /// <param name="index">The device light id.</param>
    /// <param name="color">The <see cref="Color"/> to apply.</param>
    /// <returns>True, if the color changed properly.</returns>
    public override bool SetColor(uint index, Color color)
    {
        if (!_isReady)
            Reset();

        KeyValuePair<byte, byte>[] mods = new KeyValuePair<byte, byte>[]
        {
            new KeyValuePair<byte, byte>(3, color.R),
            new KeyValuePair<byte, byte>(4, color.G),
            new KeyValuePair<byte, byte>(5, color.B),
            new KeyValuePair<byte, byte>(7, 1),
            new KeyValuePair<byte, byte>(8, (byte)index),
        };

        return PrepareAndSend(s_comm_setOneColor, mods);
    }

    /// <summary>
    /// This method apply the batched color changes to the device.
    /// </summary>
    /// <returns>True, if the colors were updated properly.</returns>
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
