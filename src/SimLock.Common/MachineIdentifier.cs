using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SimLock.Common;

/// <summary>
/// Generates a unique machine identifier based on hardware components.
/// Used for license activation tied to specific machines.
/// </summary>
public static class MachineIdentifier
{
    /// <summary>
    /// Generates a unique machine ID based on hardware fingerprint.
    /// Returns a 32-character hexadecimal string.
    /// </summary>
    public static string GetMachineId()
    {
        try
        {
            var components = new StringBuilder();

            // CPU ID
            components.Append(GetWmiValue("Win32_Processor", "ProcessorId"));

            // Motherboard Serial
            components.Append(GetWmiValue("Win32_BaseBoard", "SerialNumber"));

            // BIOS Serial
            components.Append(GetWmiValue("Win32_BIOS", "SerialNumber"));

            // First HDD Serial
            components.Append(GetWmiValue("Win32_DiskDrive", "SerialNumber"));

            // If we got no hardware info, use fallback
            if (components.Length == 0)
            {
                return GenerateFallbackId();
            }

            // Generate SHA256 hash
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(components.ToString()));

            // Return first 32 chars of hex string (16 bytes)
            return BitConverter.ToString(hash).Replace("-", "")[..32];
        }
        catch
        {
            // Fallback: Use machine name + environment info
            return GenerateFallbackId();
        }
    }

    private static string GetWmiValue(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (ManagementObject obj in searcher.Get())
            {
                var value = obj[property]?.ToString();
                if (!string.IsNullOrWhiteSpace(value) && value != "To Be Filled By O.E.M.")
                    return value;
            }
        }
        catch
        {
            // Ignore WMI errors
        }
        return "";
    }

    private static string GenerateFallbackId()
    {
        // Use environment variables as fallback
        var data = new StringBuilder();
        data.Append(Environment.MachineName);
        data.Append(Environment.UserDomainName);
        data.Append(Environment.OSVersion.ToString());
        data.Append(Environment.ProcessorCount);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data.ToString()));
        return BitConverter.ToString(hash).Replace("-", "")[..32];
    }
}
