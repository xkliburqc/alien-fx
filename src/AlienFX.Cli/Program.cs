using AlienFX;
using AlienFX.Devices;
using AlienFX.Structures;

List<AFXDevice> devices = AFX.FindDevices();

foreach (AFXDevice device in devices)
{
    RGBColor color = new()
    {
        red = (byte)Random.Shared.Next(255),
        green = (byte)Random.Shared.Next(255),
        blue = (byte)Random.Shared.Next(255),
    };

    device.SetColor(1, color, true);
    device.SetColor(2, color, true);
    device.SetColor(3, color, true);

    Console.WriteLine(device.ToString());
}
