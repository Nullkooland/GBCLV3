using System;
using System.Runtime.InteropServices;

namespace GBCLV3.Utils.Native
{
    public static class MemoryStatusUtil
    {
        #region Public Methods

        public static uint GetAvailablePhysicalMemoryMB()
        {
            var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf(typeof(MemoryStatusEx)) };
            GlobalMemoryStatusEx(ref status);
            return (uint)(status.AvailablePhysicalMemory / (1024 * 1024));
        }

        public static uint GetRecommendedMemoryMB() =>
            Math.Max((uint)Math.Pow(2.0, Math.Floor(Math.Log(GetAvailablePhysicalMemoryMB(), 2.0))), 1024);

        #endregion

        #region Native Interop

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatusEx
        {
            public uint Length;
            public uint MemoryLoad;
            public ulong TotalPhysicalMemory;
            public ulong AvailablePhysicalMemory;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtualMemory;
            public ulong AvailableVirtualMemory;
            public ulong AvailableExtendedVirtualMemory;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

        #endregion

    }
}
