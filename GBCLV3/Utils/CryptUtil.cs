using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GBCLV3.Utils
{
    /// <summary>
    ///
    /// </summary>
    static class CryptUtil
    {
        private const string _key = "🐮🍺😹🍻";
        private const string _iv = "🤣🔫🐸🕶";

        public static string EncryptString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            byte[] key = Encoding.Unicode.GetBytes(_key);
            byte[] iv = Encoding.Unicode.GetBytes(_iv);
            byte[] data = Encoding.Default.GetBytes(str);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider() { Mode = CipherMode.CFB })
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string DecryptString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            byte[] key = Encoding.Unicode.GetBytes(_key);
            byte[] iv = Encoding.Unicode.GetBytes(_iv);
            byte[] data = Convert.FromBase64String(str);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider() { Mode = CipherMode.CFB })
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                try
                {
                    cs.FlushFinalBlock();
                }
                catch
                {
                    return null;
                }
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public static string GetStringMD5(string str)
        {
            using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] md5Bytes = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sb = new StringBuilder(32);

                foreach (byte b in md5Bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string GetFileSHA1(string path)
        {
            using (SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider())
            {
                byte[] sha1Bytes = sha1Provider.ComputeHash(File.OpenRead(path));
                StringBuilder sb = new StringBuilder(40);

                foreach (byte b in sha1Bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}