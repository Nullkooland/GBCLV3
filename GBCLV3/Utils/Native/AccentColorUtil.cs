using System;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace GBCLV3.Utils.Native
{
    public static class AccentColorUtil
    {
        #region Public Methods

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

        #region Native Interop

        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        private static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);

        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        private static extern uint GetImmersiveColorTypeFromName(IntPtr pName);

        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        private static extern uint GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        #endregion
    }
}
