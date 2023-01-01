using System.Drawing;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;

/// <summary>
/// Class <c>AFXDevice</c> models a device that support AlienFX API.
/// <remarks>
/// An <c>AFXDevice</c> is the abstraction of an API version and doesn't
/// implement device specific logic. A device should be created using
/// the <see cref="AFXDeviceFactory"/> factory.
/// </remarks>
/// </summary>
public abstract class AFXDevice
{
    private readonly string _devicePath;
    private readonly int _vendorId;
    private readonly int _productId;

    /// <summary>Whether the device is ready to receive commands.</summary>
    protected bool _isReady = false;
    /// <summary>The device <see cref="SafeFileHandle"/> to send command to.</summary>
    protected SafeFileHandle _handle = new();
    /// <summary>The buffer length of the device.</summary>
    protected short _length = -1;
    /// <summary>The AlienFX api version used for this device.</summary>
    protected int _apiVersion = 0;
    /// <summary>The report id for this device. Can be 0.</summary>
    protected byte _reportId = 0;

    /// <summary>
    /// Gets the device type as <c>string</c>. Possible values are:
    /// <list type="bullet">
    /// <item><description>Desktop</description></item>
    /// <item><description>Notebook</description></item>
    /// <item><description>Desktop/Notebook</description></item>
    /// <item><description>Keyboard</description></item>
    /// <item><description>Display</description></item>
    /// <item><description>Mouse</description></item>
    /// <item><description>Unknown</description></item>
    /// </list>
    /// </summary>
    public string DeviceType => AFX.GetDeviceType(_apiVersion);
    /// <summary>This device's vendor id.</summary>
    public int VendorId => _vendorId;
    /// <summary>This device's product id.</summary>
    public int ProductId => _productId;
    /// <summary>The hardware path of a device.</summary>
    public string DevicePath => _devicePath;

    /// <summary>
    /// This constructor initializes the new Device to
    /// (<paramref name="handle"/>,
    /// <paramref name="length"/>,
    /// <paramref name="devicePath"/>,
    /// <paramref name="vid"/>,
    /// <paramref name="pid"/>,
    /// <paramref name="apiVersion"/>,
    /// <paramref name="reportId"/>).
    /// <remarks>
    /// A device should be initialized using
    /// the <see cref="AFXDeviceFactory"/> factory.
    /// </remarks>
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="length">The device buffer size.</param>
    /// <param name="devicePath">The hardware path of a device.</param>
    /// <param name="vid">The device's vendor id.</param>
    /// <param name="pid">The device's product id.</param>
    /// <param name="apiVersion">The api version used for this device.</param>
    /// <param name="reportId">A reportId sometime used with native calls.</param>
    protected AFXDevice(
        SafeFileHandle handle,
        short length,
        string devicePath,
        int vid,
        int pid,
        int apiVersion,
        byte reportId)
    {
        _handle = handle;
        _length = length;
        _devicePath = devicePath;
        _vendorId = vid;
        _productId = pid;
        _apiVersion = apiVersion;
        _reportId = reportId;
    }

    /// <summary>
    /// This method prepare and AlienFX command with an optional modifier
    /// then send it to the device through the AlienFX api.
    /// </summary>
    /// <param name="command">A command to be understood by the api.</param>
    /// <param name="mod">A <see cref="KeyValuePair{TKey, TValue}"/> modifier</param>
    /// <returns>True, if the command was sent properly.</returns>
    protected bool PrepareAndSend(byte[] command, KeyValuePair<byte, byte> mod)
    {
        return PrepareAndSend(command, new KeyValuePair<byte, byte>[] { mod });
    }

    /// <summary>
    /// This method prepare and AlienFX command with optional modifiers
    /// then send it to the device through the AlienFX api.
    /// </summary>
    /// <param name="command">A command to be understood by the api.</param>
    /// <param name="mods">An array of <see cref="KeyValuePair{TKey, TValue}"/> modifiers.</param>
    /// <returns>True, if the command was sent properly.</returns>
    protected bool PrepareAndSend(byte[] command, KeyValuePair<byte, byte>[]? mods = null)
    {
        byte[] buffer = new byte[1024];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = ((byte)(_apiVersion == 6 && buffer.Length != 3 ? 0xff : 0));
        }

        buffer[0] = _reportId;

        for (int i = 1; i <= command.Length; i++)
        {
            buffer[i] = command[i - 1];
        }

        if (mods != null)
        {
            foreach (KeyValuePair<byte, byte> mod in mods!)
            {
                buffer[mod.Key] = mod.Value;
            }
        }

        return SendCommand(buffer);
    }

    /// <summary>
    /// This method get the AlienFX device status.
    /// </summary>
    /// <returns>The device status value.</returns>
    protected byte GetStatus()
    {
        byte[] buffer = new byte[1024];
        buffer[0] = _reportId;

        return GetStatus(buffer);
    }

    /// <summary>
    /// This method send and AlienFX command with optional modifiers
    /// through the AlienFX api. The command and modifiers are bundled in a buffer.
    /// </summary>
    /// <param name="buffer">A <c>byte array</c> containing the command and optional modifiers.</param>
    /// <returns>True, if the command was sent properly.</returns>
    protected abstract bool SendCommand(byte[] buffer);

    /// <summary>
    /// This method get the AlienFX device status.
    /// </summary>
    /// <param name="buffer">A buffer containing the device report id.</param>
    /// <returns>The device status value.</returns>
    protected abstract byte GetStatus(byte[] buffer);

    /// <summary>
    /// This method send a loop command through the api.
    /// </summary>
    protected abstract void Loop();

    /// <summary>
    /// This method reset the device status to allow changes.
    /// </summary>
    /// <returns>True, if the device is properly reset.</returns>
    protected abstract bool Reset();

    /// <summary>
    /// This method apply the batched color changes to the device.
    /// </summary>
    /// <returns>True, if the colors were updated properly.</returns>
    protected abstract bool UpdateColors();

    /// <summary>
    /// This method change the RGB color of a specific device light.
    /// </summary>
    /// <param name="index">The device light id.</param>
    /// <param name="color">The <see cref="Color"/> to apply.</param>
    /// <returns>True, if the color changed properly.</returns>
    public abstract bool SetColor(uint index, Color color);

    /// <summary>
    /// This method set the brightness to all the device's lights.
    /// </summary>
    /// <param name="brightness">A <c>byte</c> value for the brightness.</param>
    /// <returns>True, if the brightness was updated properly.</returns>
    public abstract bool SetBrightness(byte brightness);

    /// <summary>
    /// Returns a <c>string</c> that represents the AlienFX device.
    /// </summary>
    /// <returns>A <c>string</c> representing a device's description.</returns>
    public override string ToString()
    {
        return $"{DeviceType} Device VID#{VendorId} PID#{ProductId} ApiV{_apiVersion} Device Path: {DevicePath}";
    }
}
