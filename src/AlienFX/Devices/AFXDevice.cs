using AlienFX.Structures;
using Microsoft.Win32.SafeHandles;

namespace AlienFX.Devices;

/// <summary>
/// <code>AFXDevice</code> class.
/// </summary>
public abstract class AFXDevice
{
    protected SafeFileHandle _handle = new();
    protected short _length = -1;
    private readonly int _version = -1;
    private readonly byte _reportId = 0;
    private readonly string _devicePath;
    private readonly int _vendorId;
    private readonly int _productId;
    protected bool _isReady = false;

    public string DeviceType => AFX.GetDeviceType(_version);
    public int VendorId => _vendorId;
    public int ProductId => _productId;
    public string DevicePath => _devicePath;

    public AFXDevice(
        SafeFileHandle handle,
        short length,
        int version,
        byte reportId,
        string devicePath,
        int vid,
        int pid)
    {
        _handle = handle;
        _length = length;
        _version = version;
        _reportId = reportId;
        _devicePath = devicePath;
        _vendorId = vid;
        _productId = pid;
    }

    protected bool PrepareAndSend(byte[] command, AFXCommand[]? mods = null)
    {
        byte[] buffer = new byte[1024];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = ((byte)(_version == 6 && buffer.Length != 3 ? 0xff : 0));
        }

        buffer[0] = _reportId;

        for (int i = 1; i <= command.Length; i++)
        {
            buffer[i] = command[i - 1];
        }

        if (mods != null)
        {
            foreach (AFXCommand mod in mods!)
            {
                buffer[mod.index] = mod.value;
            }
        }

        return PrepareAndSend(buffer);
    }

    protected byte GetStatus()
    {
        byte[] buffer = new byte[1024];
        buffer[0] = _reportId;

        return GetStatus(buffer);
    }

    /*public bool SetColor(uint index, RGBColor rgb, bool loop)
    {
        if (!_isReady)
            Reset();

        bool val = SetColor(index, rgb);

        if (loop)
            Loop();

        return val;
    }*/

    protected abstract bool PrepareAndSend(byte[] buffer);
    protected abstract byte GetStatus(byte[] buffer);
    protected abstract void Loop();
    protected abstract bool Reset();
    protected abstract bool SetColor(uint index, RGBColor color);
    protected abstract bool UpdateColors();
    public abstract bool SetBrightness(byte brightness, bool power);

    public override string ToString()
    {
        return $"{DeviceType} Device VID#{VendorId} PID#{ProductId} ApiV{_version} Device Path: {DevicePath}";
    }
}
