using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GBCLV3.Utils
{
    static class CryptUtil
    {
        public static string Guid => System.Guid.NewGuid().ToString("N");

        public static string GetStringMD5(string str)
        {
            using var md5Provider = new MD5CryptoServiceProvider();
            byte[] md5Bytes = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder(32);

            foreach (byte b in md5Bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetFileSHA1(string path)
        {
            using var sha1Provider = new SHA1CryptoServiceProvider();
            byte[] sha1Bytes = sha1Provider.ComputeHash(File.OpenRead(path));
            var sb = new StringBuilder(40);

            foreach (byte b in sha1Bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}