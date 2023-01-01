using System.Drawing;
using AlienFX.Invoke;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;

/// <summary>
/// Class <c>AFXDeviceApiV5</c> models a device that support AlienFX API version 5.
/// <remarks>
/// An <c>AFXDeviceApiV5</c> implement device specific logic.
/// A device should be created using the <see cref="AFXDeviceFactory"/> factory.
/// </remarks>
/// </summary>
public sealed class AFXDeviceApiV5 : AFXDevice
{
    private static readonly byte[] s_comm_status = new byte[] { 0x93 };
    private static readonly byte[] s_comm_colorSet = new byte[] { 0x8c, 0x02 };
    private static readonly byte[] s_comm_loop = new byte[] { 0x8c, 0x13 };
    private static readonly byte[] s_comm_reset = new byte[] { 0x94 };
    private static readonly byte[] s_comm_turnOnInit = new byte[]
		{0x79,0x7b,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
		0xff,0xff,0xff,0xff,0xff,0xff,0x7c,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
		0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0x87,0xff,0xff,0xff,0x00,0xff,
		0xff,0xff,0x00,0xff,0xff,0xff,0x00,0x77};
    private static readonly byte[] s_comm_turnOnInit2 = new byte[] { 0x79, 0x88 };
    private static readonly byte[] s_comm_turnOnSet = new byte[] { 0x83, 0x38, 0x9c };
    private static readonly byte[] s_comm_update = new byte[] { 0x8b, 0x01, 0xff };

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
    internal AFXDeviceApiV5(SafeFileHandle handle, short length, string devicePath, int vid, int pid)
        : base(handle, length, devicePath, vid, pid, 5, 0xcc)
    {
    }

    /// <summary>
    /// This method set the brightness to all the device's lights.
    /// </summary>
    /// <param name="brightness">A <c>byte</c> value for the brightness.</param>
    /// <returns>True, if the brightness was updated properly.</returns>
    public override bool SetBrightness(byte brightness)
    {
        if (_isReady)
        {
            UpdateColors();
        }

        Reset();

        _ = PrepareAndSend(s_comm_turnOnInit, null);
        _ = PrepareAndSend(s_comm_turnOnInit2, null);
        bool success = PrepareAndSend(s_comm_turnOnSet, new KeyValuePair<byte, byte>(4, brightness));

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

        PrepareAndSend(s_comm_status, null);
        buffer[1] = 0x93;
        if (Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_GET_FEATURE, 0, 0, buffer, _length, ref written, IntPtr.Zero))
            return buffer[2];

        return 0;
    }

    /// <summary>
    /// This method send a loop command through the api.
    /// </summary>
    protected override void Loop() => PrepareAndSend(s_comm_loop, null);

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

        return Kernel32.DeviceIoControl(_handle, EIOControlCode.IOCTL_HID_SET_FEATURE, buffer, _length, 0, 0, ref written, IntPtr.Zero);
    }

    /// <summary>
    /// This method reset the device status to allow changes.
    /// </summary>
    /// <returns>True, if the device is properly reset.</returns>
    protected override bool Reset()
    {
        bool result = PrepareAndSend(s_comm_reset);
        _ = base.GetStatus();

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
            new KeyValuePair<byte, byte>(4, (byte)(index + 1)),
            new KeyValuePair<byte, byte>(5, color.R),
            new KeyValuePair<byte, byte>(6, color.G),
            new KeyValuePair<byte, byte>(7, color.B),
        };

        bool success = PrepareAndSend(s_comm_colorSet, mods);

        Loop();

        return success;
    }

    /// <summary>
    /// This method apply the batched color changes to the device.
    /// </summary>
    /// <returns>True, if the colors were updated properly.</returns>
    protected override bool UpdateColors()
    {
        bool res = base.PrepareAndSend(s_comm_update, null);

        if (res)
        {
            _isReady = false;
        }

        return res;
    }
}
