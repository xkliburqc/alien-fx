using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace AlienFX.Hub.Shared;

internal static class SystemManagement
{
    public static IEnumerable<SystemDetails> GetComputerModel()
    {
        List<SystemDetails> systemDetails = new();
        SelectQuery query = new(@"Select * from Win32_ComputerSystem");

        using (ManagementObjectSearcher searcher = new(query))
        {
            foreach (ManagementObject process in searcher.Get().Cast<ManagementObject>())
            {
                process.Get();
                systemDetails.Add(new SystemDetails
                {
                    Manufacturer = process["Manufacturer"].ToString()!,
                    Model = process["Model"].ToString()!
                });
            }
        }

        return systemDetails;
    }
}

internal struct SystemDetails
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
}
