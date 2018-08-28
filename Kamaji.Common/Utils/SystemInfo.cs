namespace Kamaji.Common
{
    using System;
    using System.Net;
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    public static class SystemInfo
    {
        public static IPAddress GetIpv4Address()
             => Dns.GetHostEntry(String.Empty).AddressList.FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork);

        public static string ComputerName() => Environment.MachineName;

        public static int ProcessorCount => Environment.ProcessorCount;

        public static IResourceMonitor CreateResourceMonitor()
        {
            if (OperatingSystem.IsWindows())
            {
                return new WindowsResourceMonitor();
            }

            throw new NotSupportedException(RuntimeInformation.OSDescription);
        }
    }


    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
