using System;
using System.Globalization;
using System.Runtime.InteropServices;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace NickStrupat
{
    /// <summary>
    /// Operating system enum
    /// </summary>
    public enum EOSPlatform
    {
        Windows,
        MacOS,
        Linux
    }

    public class ComputerInfo
    {
        // ME3Tweaks =============
        /// <summary>
        /// Force overrides the platform
        /// </summary>
        /// <param name="platform"></param>
        public static void ForcePlatform(EOSPlatform platform)
        {
            IsWindows = platform == EOSPlatform.Windows;
            IsMacOS = platform == EOSPlatform.MacOS;
            IsLinux = platform == EOSPlatform.Linux;
        }

        // End ME3Tweaks =========
        static ComputerInfo()
        {
            if (IsWindows)
            {
                GetTotalPhysicalMemory = Windows.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = Windows.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory = Windows.GetTotalVirtualMemory;
                GetAvailableVirtualMemory = Windows.GetAvailableVirtualMemory;
                GetMemorySpeed = Windows.GetMemorySpeed;
                GetCPUVendor = Windows.CPUVendor;
                GetCPUName = Windows.CPUName;
                IsActuallyPlatform = Windows.IsActuallyWindows;
            }
            else if (IsMacOS)
            {
                GetTotalPhysicalMemory = MacOS.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = MacOS.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory = MacOS.GetTotalVirtualMemory;
                GetAvailableVirtualMemory = MacOS.GetAvailableVirtualMemory;
                GetMemorySpeed = () => 0; // Not supported
                GetCPUVendor = () => "Not supported";
                GetCPUName = () => "Not supported";
                IsActuallyPlatform = () => false; // This is not Windows
            }
            else if (IsLinux)
            {
                GetTotalPhysicalMemory = Linux.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = Linux.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory = Linux.GetTotalVirtualMemory;
                GetAvailableVirtualMemory = Linux.GetAvailableVirtualMemory;
                GetMemorySpeed = () => 0; // Not supported
                GetCPUVendor = Linux.CPUVendor;
                GetCPUName = Linux.CPUName;
                IsActuallyPlatform = () => false; // This is not Windows
            }
            else
                throw new PlatformNotSupportedException();
        }

        // ME3Tweaks - these used to be marked readonly, removed to support overrides
        private static Boolean IsWindows = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        private static Boolean IsMacOS = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
        private static Boolean IsLinux = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);

        private static readonly Func<UInt64> GetTotalPhysicalMemory;
        private static readonly Func<UInt64> GetAvailablePhysicalMemory;
        private static readonly Func<UInt64> GetTotalVirtualMemory;
        private static readonly Func<UInt64> GetAvailableVirtualMemory;
        private static readonly Func<String> GetCPUVendor;
        private static readonly Func<int> GetMemorySpeed;
        private static readonly Func<String> GetCPUName;
        private static readonly Func<bool> IsActuallyPlatform;

        public UInt64 TotalPhysicalMemory => GetTotalPhysicalMemory.Invoke();
        public UInt64 AvailablePhysicalMemory => GetAvailablePhysicalMemory.Invoke();
        public UInt64 TotalVirtualMemory => GetTotalVirtualMemory.Invoke();
        public UInt64 AvailableVirtualMemory => GetAvailableVirtualMemory.Invoke();
        public int MemorySpeed => GetMemorySpeed.Invoke();
        public String CPUVendor => GetCPUVendor.Invoke();
        public String CPUName => GetCPUName.Invoke();

        /// <summary>
        /// If this OS is actually what it seems to be (and is not being emulated)
        /// </summary>
        public bool ActuallyPlatform => IsActuallyPlatform.Invoke();

        public CultureInfo InstalledUICulture => CultureInfo.InstalledUICulture;
        public String OSFullName => IsWindows ? Windows.OSFullName : RuntimeEnvironment.OperatingSystem + " " + RuntimeEnvironment.OperatingSystemVersion;
        public String OSPlatform => Environment.OSVersion.Platform.ToString();
        public String OSVersion => Environment.OSVersion.Version.ToString();
    }
}
