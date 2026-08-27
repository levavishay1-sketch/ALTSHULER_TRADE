using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Alt.Framework.Utils
{
    public static class StringUtils
    {
        public static string Random(int length = 100)
        {
            Random random = new Random();
            string chars = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetUniqueKey(int size)
        {
            char[] chars =
                "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static string SafeTrimStartingZeros(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.TrimStart('0').Trim();
        }
    }
}
