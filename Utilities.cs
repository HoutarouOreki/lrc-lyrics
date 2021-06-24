using System;
using System.Security.Cryptography;
using System.Text;

namespace LrcLyrics
{
    public static class Utilities
    {
        private static string connectionStringMlab => $"{Environment.GetEnvironmentVariable("LRC_LYRICS_DATABASE_URI")}/lrc-lyrics";
        public static string ConnectionString => $"{connectionStringMlab}?retryWrites=false";

        public static void AppendLyricTag(this StringBuilder s, string tag, string value)
        {
            if (!string.IsNullOrEmpty(value))
                s.AppendLine($"[{tag}:{value}]");
        }

        public static string KeyGenerator(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = new StringBuilder(length);
            using (var rng = new RNGCryptoServiceProvider())
            {
                var count = (int)Math.Ceiling(Math.Log(alphabet.Length, 2) / 8.0);
                var offset = BitConverter.IsLittleEndian ? 0 : sizeof(uint) - count;
                var max = (int)(Math.Pow(2, count * 8) / alphabet.Length) * alphabet.Length;
                var uintBuffer = new byte[sizeof(uint)];

                while (res.Length < length)
                {
                    rng.GetBytes(uintBuffer, offset, count);
                    var num = BitConverter.ToUInt32(uintBuffer, 0);
                    if (num < max)
                    {
                        res.Append(alphabet[(int)(num % alphabet.Length)]);
                    }
                }
            }

            return res.ToString();
        }
    }
}
