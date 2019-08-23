using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace GBCLV3.Utils
{
    static class NativeUtil
    {
        #region Public Methods

        public static void EnableBlur(IntPtr hwnd)
        {
            var accent = new AccentPolicy();
            int accentStructSize = Marshal.SizeOf(accent);

            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            //accent.AccentFlags = 0x20 | 0x40 | 0x80 | 0x100;
            //accent.GradientColor = 0x99FFFFFF;
            accent.GradientColor = 0x00FFFFFF;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(hwnd, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public static Color GetSystemColorByName(string colorName)
        {
            uint colorSetEx = GetImmersiveColorFromColorSetEx(
                GetImmersiveUserColorSetPreference(false, false),
                GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni(colorName)),
                false, 0);

            return Color.FromArgb(
                (byte)((0xFF000000 & colorSetEx) >> 24),
                (byte)(0x000000FF & colorSetEx),
                (byte)((0x0000FF00 & colorSetEx) >> 8),
                (byte)((0x00FF0000 & colorSetEx) >> 16)
            );
        }

        #endregion

        #region Window Blur

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        #endregion

        #region System Accent Color

        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        private static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);

        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        private static extern uint GetImmersiveColorTypeFromName(IntPtr pName);

        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        private static extern uint GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        #endregion
    }
}
