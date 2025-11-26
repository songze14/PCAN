using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static class ExICryptoTransform
    {
        public static string EncryptHexString(this ICryptoTransform crypto, string plainText) => Convert.ToHexString(EncryptString(crypto,plainText));
        public static string DecryptHexString(this ICryptoTransform crypto, string plainText) => DecryptString(crypto, Convert.FromHexString(plainText));
        private static byte[] EncryptString(this ICryptoTransform crypto, string plainText)
        {
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return ms.ToArray();
        }
        private static string DecryptString(this ICryptoTransform crypto, byte[] cipherText)
        {
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
