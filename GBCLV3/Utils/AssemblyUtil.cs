using System;
using System.Reflection;

namespace GBCLV3.Utils
{
    internal static class AssemblyUtil
    {
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        public static string Title =>
            (Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute), false)
            as AssemblyTitleAttribute).Title;

        public static string Version =>
            (Attribute.GetCustomAttribute(_assembly, typeof(AssemblyFileVersionAttribute), false)
            as AssemblyFileVersionAttribute).Version;

        public static string Copyright =>
            (Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCopyrightAttribute), false)
            as AssemblyCopyrightAttribute).Copyright;
    }
}
