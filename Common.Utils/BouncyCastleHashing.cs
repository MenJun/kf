// *************************************************************
//
// 文件名(File Name)：BouncyCastleHashing
//
// 功能描述(Description)：PBKDF2_SHA256加密工具类
// 
// 作者(Author)：陈光华
//
// 创建日期(Create Date)：2018/1/27 10:08:49
//
// 修改记录(Revision History)：
//		R1：
//			修改作者：
//			修改日期：
//			修改描述：
//
//		R2：
//			修改作者：
//			修改日期：
//			修改描述：
//
// *************************************************************

using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Common.Utils
{
    /// <summary>
    /// 加密算法
    /// </summary>
    public static class BouncyCastleHashing
    {
        private static SecureRandom _cryptoRandom;
        /// <summary>
        /// 加密次数
        /// </summary>
        private const int iterations = 10000;
        /// <summary>
        /// salt size
        /// </summary>
        private const int saltByteSize = 64;
        /// <summary>
        /// final hash size
        /// </summary>
        private const int hashByteSize = 128;

        static BouncyCastleHashing()
        {
            _cryptoRandom  = new SecureRandom();
        }

        /// <summary>
        /// 生成随机盐
        /// </summary>
        /// <param name="size">The size of the salt in bytes</param>
        /// <returns>A random salt of the required size.</returns>
        public static byte[] CreateSalt()
        {
            byte[] salt = new byte[saltByteSize];
            _cryptoRandom.NextBytes(salt);
            return salt;
        }

        /// <summary>
        /// 生成随机盐
        /// </summary>
        /// <param name="size">The size of the salt in bytes</param>
        /// <returns>A random salt of the required size.</returns>
        public static byte[] CreateSalt(int size)
        {
            byte[] salt = new byte[size];
            _cryptoRandom.NextBytes(salt);
            return salt;
        }

        /// <summary>
        /// Gets a PBKDF2_SHA256 Hash  (Overload)
        /// </summary>
        /// <param name="password">The password as a plain text string</param>
        /// <param name="saltAsBase64String">The salt for the password</param>
        /// <returns>A base64 string of the hash.</returns>
        public static string PBKDF2_SHA256_GetHash(string str, string saltAsBase64String)
        {
            var saltBytes = Convert.FromBase64String(saltAsBase64String);

            var hash = PBKDF2_SHA256_GetHash(str, saltBytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Gets a PBKDF2_SHA256 Hash (CORE METHOD)
        /// </summary>
        /// <param name="password">The password as a plain text string</param>
        /// <param name="salt">The salt as a byte array</param>
        /// <returns>A the hash as a byte array.</returns>
        public static byte[] PBKDF2_SHA256_GetHash(string str, byte[] salt)
        {
            var pdb = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
            pdb.Init(PbeParametersGenerator.Pkcs5PasswordToBytes(str.ToCharArray()), salt, iterations);
            var key = (KeyParameter)pdb.GenerateDerivedMacParameters(hashByteSize * 8);
            return key.GetKey();
        }

        /// <summary>
        /// Validates a password given a hash of the correct one. (OVERLOAD)
        /// </summary>
        /// <param name="password">The original password to hash</param>
        /// <param name="salt">The salt that was used when hashing the password</param>
        /// <param name="hashAsBase64String">The hash the password previously provided as a base64 string</param>
        /// <returns>True if the hashes match</returns>
        public static bool ValidatePassword(string password, string salt, string hashAsBase64String)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] actualHashBytes = Convert.FromBase64String(hashAsBase64String);
            return ValidatePassword(password, saltBytes, actualHashBytes);
        }

        /// <summary>
        /// Validates a password given a hash of the correct one (MAIN METHOD).
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <param name="correctHash">A hash of the correct password.</param>
        /// <returns>True if the password is correct. False otherwise.</returns>
        public static bool ValidatePassword(string password, byte[] saltBytes, byte[] actualGainedHasAsByteArray)
        {
            byte[] testHash = PBKDF2_SHA256_GetHash(password, saltBytes);
            return SlowEquals(actualGainedHasAsByteArray, testHash);
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        /// <summary>
        /// 实例
        /// </summary>
        /// <param name="password">密码明文</param>
        /// <returns>返回加密后的字符串</returns>
        public static string EncryptionPassword(string password, byte[] saltBytes)
        {
            string saltString = Convert.ToBase64String(saltBytes);

            string pwdHash = PBKDF2_SHA256_GetHash(password, saltString);

            return pwdHash;
        }
    }
}
