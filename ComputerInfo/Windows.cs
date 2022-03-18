using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace NickStrupat
{
    internal static class Windows
    {
        public static UInt64 GetTotalPhysicalMemory() => MemoryStatus.TotalPhysicalMemory;
        public static UInt64 GetAvailablePhysicalMemory() => MemoryStatus.AvailablePhysicalMemory;
        public static UInt64 GetTotalVirtualMemory() => MemoryStatus.TotalVirtualMemory;
        public static UInt64 GetAvailableVirtualMemory() => MemoryStatus.AvailableVirtualMemory;
        public static int GetMemorySpeed() => MemoryStatus.MemorySpeed;

        //We call trim as if ReleaseId is not set (Windows 8.1) it will trim the data off.
        public static String OSFullName = ($"Microsoft {Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion").GetValue("ProductName")} {Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion").GetValue("ReleaseId")}").Trim();

        public static String CPUVendor()
        {
            try
            {
                ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_Processor");
                foreach (ManagementObject moProcessor in mosProcessor.Get())
                {
                    if (moProcessor["Manufacturer"] != null)
                    {
                        return moProcessor["Manufacturer"].ToString().Trim();
                    }
                }
            }
            catch
            {
            }
            return null;

        }

        public static String CPUName()
        {
            try
            {
                ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT NAME FROM Win32_Processor");
                foreach (ManagementObject moProcessor in mosProcessor.Get())
                {
                    if (moProcessor["Name"] != null)
                    {
                        return moProcessor["Name"].ToString().Trim();
                    }
                }
            }
            catch
            {
            }
            return null;
        }


        private static InternalMemoryStatus internalMemoryStatus;
        private static InternalMemoryStatus MemoryStatus => internalMemoryStatus ?? (internalMemoryStatus = new InternalMemoryStatus());


        private class InternalMemoryStatus
        {
            private readonly Boolean isOldOS;
            private MEMORYSTATUS memoryStatus;
            private MEMORYSTATUSEX memoryStatusEx;
            private int _memorySpeed;

            internal InternalMemoryStatus()
            {
                isOldOS = Environment.OSVersion.Version.Major < 5;
            }

            internal UInt64 TotalPhysicalMemory
            {
                get
                {
                    Refresh();
                    return !isOldOS ? memoryStatusEx.ullTotalPhys : memoryStatus.dwTotalPhys;
                }
            }

            internal UInt64 AvailablePhysicalMemory
            {
                get
                {
                    Refresh();
                    return !isOldOS ? memoryStatusEx.ullAvailPhys : memoryStatus.dwAvailPhys;
                }
            }

            internal UInt64 TotalVirtualMemory
            {
                get
                {
                    Refresh();
                    return !isOldOS ? memoryStatusEx.ullTotalVirtual : memoryStatus.dwTotalVirtual;
                }
            }

            internal UInt64 AvailableVirtualMemory
            {
                get
                {
                    Refresh();
                    return !isOldOS ? memoryStatusEx.ullAvailVirtual : memoryStatus.dwAvailVirtual;
                }
            }

            internal int MemorySpeed
            {
                get
                {
                    if (_memorySpeed > 0) return _memorySpeed;

                    ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT ConfiguredClockSpeed FROM Win32_PhysicalMemory");
                    foreach (ManagementObject moProcessor in mosProcessor.Get())
                    {
                        if (moProcessor["ConfiguredClockSpeed"] != null && int.TryParse(moProcessor["ConfiguredClockSpeed"].ToString(), out _memorySpeed))
                        {
                            // This is not used.
                            break;
                        }
                    }

                    return _memorySpeed;
                }
            }

            private void Refresh()
            {
                if (isOldOS)
                {
                    memoryStatus = new MEMORYSTATUS();
                    GlobalMemoryStatus(ref memoryStatus);
                }
                else
                {
                    memoryStatusEx = new MEMORYSTATUSEX();
                    memoryStatusEx.Init();
                    if (!GlobalMemoryStatusEx(ref memoryStatusEx))
                        throw new Win32Exception("Could not obtain memory information due to internal error.");
                }
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern void GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        internal struct MEMORYSTATUS
        {
            internal UInt32 dwLength;
            internal UInt32 dwMemoryLoad;
            internal UInt32 dwTotalPhys;
            internal UInt32 dwAvailPhys;
            internal UInt32 dwTotalPageFile;
            internal UInt32 dwAvailPageFile;
            internal UInt32 dwTotalVirtual;
            internal UInt32 dwAvailVirtual;
        }

        internal struct MEMORYSTATUSEX
        {
            internal UInt32 dwLength;
            internal UInt32 dwMemoryLoad;
            internal UInt64 ullTotalPhys;
            internal UInt64 ullAvailPhys;
            internal UInt64 ullTotalPageFile;
            internal UInt64 ullAvailPageFile;
            internal UInt64 ullTotalVirtual;
            internal UInt64 ullAvailVirtual;
            internal UInt64 ullAvailExtendedVirtual;

            internal void Init()
            {
                dwLength = checked((UInt32)Marshal.SizeOf(typeof(MEMORYSTATUSEX)));
            }
        }
    }
}
