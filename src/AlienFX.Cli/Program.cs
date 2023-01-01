using System.Drawing;
using AlienFX;
using AlienFX.Devices;

List<AFXDevice> devices = AFX.FindDevices();

foreach (AFXDevice device in devices)
{
    Console.WriteLine(device.ToString());

    Color red = Color.FromArgb(255, 0, 0);
    Color green = Color.FromArgb(0, 255, 0);

    for (uint i = 0; i < 200; i++)
    {
        device.SetColor(i, red);
    }

    for (uint i = 0; i < 136; i++)
    {
        device.SetColor(i, green);
        if(i > 0)
        {
            device.SetColor(i-1, red);
        }
        Console.WriteLine($"Showing index: {i}");
        Console.ReadKey();
    }

    device.SetBrightness(255);
}

//AFX.GetCurrentPowerProfile();
