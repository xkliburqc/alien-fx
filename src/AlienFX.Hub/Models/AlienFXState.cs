using AlienFX.Invoke;

namespace AlienFX.Hub.Models;
internal class AlienFXState
{
    private byte _brightness = 255;
    private bool _isAlienFXOn = true;
    private ACLineStatus _acLineStatus = ACLineStatus.Unknown;

    public byte Brightness { get => _brightness; set => _brightness = value; }
    public bool IsAlienFXOn { get => _isAlienFXOn; set => _isAlienFXOn = value; }
    public ACLineStatus AcLineStatus { get => _acLineStatus; set => _acLineStatus = value; }
}
