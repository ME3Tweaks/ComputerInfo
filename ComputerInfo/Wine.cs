using System;
using System.IO;
using System.Linq;


namespace NickStrupat
{
    /// <summary>
    /// Mostly copy pasted from the Linux class, but adapted for Wine
    /// </summary>
    internal static class Wine
    {
        public static UInt64 GetTotalPhysicalMemory() => GetBytesFromLine("MemTotal:");
        public static UInt64 GetAvailablePhysicalMemory() => GetBytesFromLine("MemFree:");
        public static UInt64 GetTotalVirtualMemory() => GetBytesFromLine("SwapTotal:");
        public static UInt64 GetAvailableVirtualMemory() => GetBytesFromLine("SwapFree:");
        private static String[] GetProcMemInfoLines() => File.ReadAllLines("z:proc/meminfo");
        private static String[] GetProcCpuInfoLines() => File.ReadAllLines("z:proc/cpuinfo");
        private static String[] GetOSInfoLines() => File.ReadAllLines("z:etc/os-release");
        private static String GetHostname() => File.ReadLines("z:etc/hostname").First(); // this should only have one line anyway
        public static String CPUVendor() => GetCPUValueFromLine("vendor_id");
        public static String CPUName() => GetCPUValueFromLine("model name");
        public static String OSName() => GetOSValueFromLine("NAME");
        public static String OSVersion() => GetOSValueFromLine("VERSION_ID");
        public static String OSVersionVerbose() => GetOSValueFromLine("VERSION");
        public static String OSFullName() => GetOSValueFromLine("PRETTY_NAME");

        private static String GetCPUValueFromLine(String token) => GetProcCpuInfoLines().FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length).Trim().Substring(1).Trim();
        private static String GetOSValueFromLine(String token) => GetOSInfoLines().FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length).Trim().Substring(1).Trim().Trim('"');

        private static UInt64 GetBytesFromLine(String token)
        {
            const String KbToken = "kB";
            var memTotalLine = GetProcMemInfoLines().FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length);
            if (memTotalLine != null && memTotalLine.EndsWith(KbToken) && UInt64.TryParse(memTotalLine.Substring(0, memTotalLine.Length - KbToken.Length), out var memKb))
                return memKb * 1024;
            throw new Exception();
        }

        /// <summary>
        /// Returns list of IDs for active GPUs. VendorID:DeviceID
        /// </summary>
        /// <returns></returns>
        public static String[] GetGPUDeviceNames()
        {
            var GPUPaths = Directory.GetDirectories("z:/sys/class/drm/", "renderD1??");
            String[] devices = new String[GPUPaths.Length];
            for (int idx = 0; idx < GPUPaths.Length; idx++)
            {
                var vendor = File.ReadLines(GPUPaths[idx] + "/device/vendor").First();
                var device = File.ReadLines(GPUPaths[idx] + "/device/device").First();
                devices[idx] = vendor + ":" + device;
            }
            return devices;
        }


    }
}
