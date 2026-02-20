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
        public static UInt64 GetTotalPhysicalMemory() => GetMemInfoValue("MemTotal:");
        public static UInt64 GetAvailablePhysicalMemory() => GetMemInfoValue("MemFree:");
        public static UInt64 GetTotalVirtualMemory() => GetMemInfoValue("SwapTotal:");
        public static UInt64 GetAvailableVirtualMemory() => GetMemInfoValue("SwapFree:");
        private static String GetHostname() => File.ReadLines(@"z:/etc/hostname").First(); // this should only have one line anyway
        public static String CPUVendor() => GetCPUInfoValue("vendor_id");
        public static String CPUName() => GetCPUInfoValue("model name");
        public static UInt32 CPUMaxClock() => GetCPUClock("max", 0) ?? 0;
        public static UInt32 CPUAvgClock() => GetCPUClock("avg", 0) ?? 0;
        public static UInt16 CPUPhysicalCores() => UInt16.Parse(GetCPUInfoValue("cpu cores") ?? "0");
        public static UInt16 CPULogicalCores() => UInt16.Parse(GetCPUInfoValue("siblings") ?? "0");
        public static String OSName() => GetOSValueFromLine("NAME");
        public static String OSVersion() => GetOSValueFromLine("VERSION_ID");
        public static String OSVersionVerbose() => GetOSValueFromLine("VERSION");
        public static String OSFullName() => GetOSValueFromLine("PRETTY_NAME");


        private static String GetOSValueFromLine(String token)
        {
            String[] OSInfoLines;
            try
            {
                OSInfoLines = File.ReadAllLines(@"z:/etc/os-release");
            }
            catch (Exception x)
            {
                if (x is DirectoryNotFoundException || x is FileNotFoundException)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return OSInfoLines.FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length).Trim().Substring(1).Trim().Trim('"');
        }

        /// <summary>
        /// Reads /proc/cpuinfo and returns value for specified token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static String GetCPUInfoValue(String token)
        {
            String[] CPUInfoLines;
            try
            {
                CPUInfoLines = File.ReadAllLines(@"z:/proc/cpuinfo");
            }
            catch (Exception x)
            {
                if (x is DirectoryNotFoundException || x is FileNotFoundException)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return CPUInfoLines.FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length).Trim().Substring(1).Trim();
        }

        /// <summary>
        /// Returns cpu frequency for specific logical core in Hertz
        /// </summary>
        /// <param name="Value">min/avg/max</param>
        /// <param name="CoreID"></param>
        /// <returns></returns>
        public static UInt32? GetCPUClock(String ValueType, UInt16 CoreID)
        {
            try
            {
                UInt32.TryParse(File.ReadLines(@$"z:/sys/devices/system/cpu/cpu{CoreID}/cpufreq/cpuinfo_{ValueType.ToLower()}_freq").First(), out var freq);
                return freq;
            }
            catch (Exception x)
            {
                if (x is DirectoryNotFoundException || x is FileNotFoundException)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads /proc/meminfo and returns value for specified token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static UInt64 GetMemInfoValue(String token)
        {
            const String KbToken = "kB";
            String[] MemInfoLines;
            try
            {
                MemInfoLines = File.ReadAllLines(@"z:/proc/meminfo");
            }
            catch (Exception x)
            {
                if (x is DirectoryNotFoundException || x is FileNotFoundException)
                {
                    return 0;
                }
                else
                {
                    throw;
                }
            }
            var TargetLine = MemInfoLines.FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length);
            if (TargetLine != null && TargetLine.EndsWith(KbToken) && UInt64.TryParse(TargetLine.Substring(0, TargetLine.Length - KbToken.Length), out var memKb))
                return memKb * 1024;
            return 0;
        }

        /// <summary>
        /// Returns CPU model name, max clock, amount of cores and logical processors
        /// </summary>
        /// <returns></returns>
        public static String ChungusCPUInfo()
        {
            String info = "";
            info += $"{CPUName}\n";
            info += $"Maximum reported clock speed: {CPUMaxClock()}";
            info += $"Cores: {CPUPhysicalCores}";
            info += $"Logical processors: {CPULogicalCores}";

            return info;
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
