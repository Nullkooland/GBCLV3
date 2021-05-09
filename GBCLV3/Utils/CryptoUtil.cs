using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GBCLV3.Utils
{
    public static class CryptoUtil
    {
        #region Private Constants

        private static readonly char[] _hexTable =
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        #endregion Region

        #region Public Methods

        public static ReadOnlySpan<byte> RemoveUtf8BOM(ReadOnlySpan<byte> data)
        {
            // Read past the UTF-8 BOM bytes if a BOM exists.
            if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                return data[3..];
            }

            return data;
        }

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

        public static bool ValidateFileSHA1(string path, ReadOnlySpan<char> sha1)
        {
            using var sha1Provider = new SHA1CryptoServiceProvider();
            using var fileStream = File.OpenRead(path);
            byte[] sha1Bytes = sha1Provider.ComputeHash(fileStream);

            for (int i = 0; i < sha1Bytes.Length; i++)
            {
                char c0 = _hexTable[sha1Bytes[i] >> 4];
                char c1 = _hexTable[sha1Bytes[i] & 0x0F];

                if (c0 != sha1[2 * i] || c1 != sha1[2 * i + 1])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateFileSHA256(string path, ReadOnlySpan<char> sha256)
        {
            using var sha1Provider = new SHA256CryptoServiceProvider();
            using var fileStream = File.OpenRead(path);
            byte[] sha256Bytes = sha1Provider.ComputeHash(fileStream);

            for (int i = 0; i < sha256Bytes.Length; i++)
            {
                char c0 = _hexTable[sha256Bytes[i] >> 4];
                char c1 = _hexTable[sha256Bytes[i] & 0x0F];

                if (c0 != sha256[2 * i] || c1 != sha256[2 * i + 1])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}