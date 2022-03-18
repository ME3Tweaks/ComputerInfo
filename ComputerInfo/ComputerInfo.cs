﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace NickStrupat
{
    public class ComputerInfo {
        static ComputerInfo() {
            if (IsWindows) {
                GetTotalPhysicalMemory     = Windows.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = Windows.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory      = Windows.GetTotalVirtualMemory;
                GetAvailableVirtualMemory  = Windows.GetAvailableVirtualMemory;
                GetMemorySpeed             = Windows.GetMemorySpeed;
                GetCPUVendor = Windows.CPUVendor;
                GetCPUName = Windows.CPUName;
            }
            else if (IsMacOS) {
                GetTotalPhysicalMemory     = MacOS.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = MacOS.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory      = MacOS.GetTotalVirtualMemory;
                GetAvailableVirtualMemory  = MacOS.GetAvailableVirtualMemory;
                GetMemorySpeed = () => 0; // Not supported
                GetCPUVendor = () => "Not supported";
                GetCPUName = () => "Not supported";
            }
            else if (IsLinux) {
                GetTotalPhysicalMemory     = Linux.GetTotalPhysicalMemory;
                GetAvailablePhysicalMemory = Linux.GetAvailablePhysicalMemory;
                GetTotalVirtualMemory      = Linux.GetTotalVirtualMemory;
                GetAvailableVirtualMemory  = Linux.GetAvailableVirtualMemory;
                GetMemorySpeed = () => 0; // Not supported
                GetCPUVendor = Linux.CPUVendor;
                GetCPUName = Linux.CPUName;
            }
            else
                throw new PlatformNotSupportedException();
        }

        private static readonly Boolean IsWindows = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        private static readonly Boolean IsMacOS = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
        private static readonly Boolean IsLinux = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);

        private static readonly Func<UInt64> GetTotalPhysicalMemory;
        private static readonly Func<UInt64> GetAvailablePhysicalMemory;
        private static readonly Func<UInt64> GetTotalVirtualMemory;
        private static readonly Func<UInt64> GetAvailableVirtualMemory;
        private static readonly Func<String> GetCPUVendor;
        private static readonly Func<int> GetMemorySpeed;
        private static readonly Func<String> GetCPUName;

        public UInt64 TotalPhysicalMemory     => GetTotalPhysicalMemory.Invoke();
        public UInt64 AvailablePhysicalMemory => GetAvailablePhysicalMemory.Invoke();
        public UInt64 TotalVirtualMemory      => GetTotalVirtualMemory.Invoke();
        public UInt64 AvailableVirtualMemory  => GetAvailableVirtualMemory.Invoke();
        public int MemorySpeed  => GetMemorySpeed.Invoke();
        public String CPUVendor => GetCPUVendor.Invoke();
        public String CPUName => GetCPUName.Invoke();

        public CultureInfo InstalledUICulture => CultureInfo.InstalledUICulture;
        public String      OSFullName         => IsWindows ? Windows.OSFullName : RuntimeEnvironment.OperatingSystem + " " + RuntimeEnvironment.OperatingSystemVersion;
        public String      OSPlatform         => Environment.OSVersion.Platform.ToString();
        public String      OSVersion          => Environment.OSVersion.Version.ToString();
    }
}
