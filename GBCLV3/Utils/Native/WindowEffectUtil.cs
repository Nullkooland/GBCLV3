using System;
using System.Runtime.InteropServices;
using GBCLV3.Models.Theme;

namespace GBCLV3.Utils.Native
{
    public static class WindowEffectUtil
    {
        #region Public Methods

        public static void Apply(IntPtr windowHandle, BackgroundEffect effect)
        {
            var accent = new AccentPolicy();
            int accentStructSize = Marshal.SizeOf(accent);

            accent.AccentState = effect switch
            {
                BackgroundEffect.SolidColor => AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT,
                BackgroundEffect.BlurBehind => AccentState.ACCENT_ENABLE_BLURBEHIND,
                BackgroundEffect.AcrylicMaterial => AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                _ => AccentState.ACCENT_ENABLE_BLURBEHIND,
            };

            //accent.AccentFlags = 0x20 | 0x40 | 0x80 | 0x100;
            //accent.GradientColor = 0x99FFFFFF;
            accent.GradientColor = 0x018B8B8B;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHandle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        #endregion

        #region Native Interop

        [Flags]
        private enum AccentState : uint
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
            public uint GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [Flags]
        private enum WindowCompositionAttribute : uint
        {
            WCA_ACCENT_POLICY = 19
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        #endregion
    }
}
