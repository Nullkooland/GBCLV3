using System.Diagnostics;

namespace GBCLV3.Utils
{
    public static class SystemUtil
    {
        public static void OpenLink(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
    }
}