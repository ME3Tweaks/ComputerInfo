using System;
using System.IO;
using System.Linq;

namespace NickStrupat
{
    internal static class Linux
    {
        public static UInt64 GetTotalPhysicalMemory() => GetBytesFromLine("MemTotal:");
        public static UInt64 GetAvailablePhysicalMemory() => GetBytesFromLine("MemFree:");
        public static UInt64 GetTotalVirtualMemory() => GetBytesFromLine("SwapTotal:");
        public static UInt64 GetAvailableVirtualMemory() => GetBytesFromLine("SwapFree:");
        private static String[] GetProcMemInfoLines() => File.ReadAllLines("/proc/meminfo");
        //private static String[] GetProcCpuInfoLines() => File.ReadAllLines("/proc/cpuinfo");
        private static String[] GetProcCpuInfoLines() => File.ReadAllLines(@"X:\Downloads\cpuinfo.txt");
        public static String CPUVendor() => GetValueFromLine("vendor_id");
        public static String CPUName() => GetValueFromLine("model name");

        private static String GetValueFromLine(String token) => GetProcCpuInfoLines().FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length).Trim().Substring(1).Trim();

        private static UInt64 GetBytesFromLine(String token)
        {
            const String KbToken = "kB";
            var memTotalLine = GetProcMemInfoLines().FirstOrDefault(x => x.StartsWith(token))?.Substring(token.Length);
            if (memTotalLine != null && memTotalLine.EndsWith(KbToken) && UInt64.TryParse(memTotalLine.Substring(0, memTotalLine.Length - KbToken.Length), out var memKb))
                return memKb * 1024;
            throw new Exception();
        }
    }
}
