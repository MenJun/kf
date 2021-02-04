using System;
using System.Security.Cryptography;

namespace Common.Utils
{
    /// <summary>
    /// 生成一个指定长度的真随机字符串
    /// </summary>
    public class UniqueKeyHelper
    {
        public static string GetUniqueKey(int size)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

            byte[] bytes = new byte[size * 8];
            
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(bytes);
            }

            var result = new char[size];

            for (int i = 0; i < size; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = chars[(int)(value % (uint)chars.Length)];
            }
            return new string(result);
        }
    }
}
