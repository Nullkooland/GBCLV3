using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GBCLV3.Utils.Native
{
    public static class RecycleBinUtil
    {
        #region Public Methods

        public static bool Send(IEnumerable<string> paths)
        {
            string pathsMerged = string.Join('\0', paths);
            try
            {
                var fs = new SHFileOpStruct
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = pathsMerged + "\0\0",
                    fFlags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING
                };

                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Native Interop

        [Flags]
        public enum FileOperationFlags : ushort
        {
            FOF_SILENT = 0x0004,
            FOF_NOCONFIRMATION = 0x0010,
            FOF_ALLOWUNDO = 0x0040,
            FOF_SIMPLEPROGRESS = 0x0100,
            FOF_NOERRORUI = 0x0400,
            FOF_WANTNUKEWARNING = 0x4000,
        }

        [Flags]
        public enum FileOperationType : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFileOpStruct
        {

            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFileOpStruct fileOp);

        #endregion
    }
}
