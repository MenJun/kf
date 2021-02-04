// *************************************************************
//
// 文件名(File Name)：AES256Decrypt
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2018/1/30 16:27:13
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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utils
{
    public class AES256Helper
    {
        /// <summary>
        /// 加密密匙
        /// 生成方式  md5(sha256(md5('shuziyu.net.2019')))
        /// </summary>
        private const string AES_KEY = "1bb7f15f63878427c3775785cef335fe";
        /// <summary>
        /// 加密向量
        /// 默认0000000000000000
        /// 前8位当前月日补0
        /// 后8位当前时补0+年后两位
        /// </summary>
        private const string AES_IV = "0114000015000019";
        /// <summary>
        /// aes256 加密
        /// </summary>
        /// <param name="encryptStr">待加密字符串</param>
        /// <returns></returns>
        public static string Encrypt(string encryptStr)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);

            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.KeySize = 256;

                rijAlg.Key = Encoding.UTF8.GetBytes(AES_KEY);
                rijAlg.IV = Encoding.UTF8.GetBytes(AES_IV);

                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                encryptStr = Convert.ToBase64String(encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length));
            }
            return encryptStr;
        }
        /// <summary>
        /// aes256 解密
        /// </summary>
        /// <param name="encryptStr">加密字符串</param>
        /// <returns></returns>
        public static string Decrypt(string encryptStr)
        {
            byte[] toEncryptArray = Convert.FromBase64String(encryptStr);

            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.KeySize = 256;

                rijAlg.Key = Encoding.UTF8.GetBytes(AES_KEY);
                rijAlg.IV = Encoding.UTF8.GetBytes(AES_IV);

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.  
                using (var msDecrypt = new MemoryStream(toEncryptArray))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {

                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream  
                            // and place them in a string.  
                            encryptStr = srDecrypt.ReadToEnd();

                        }

                    }
                }
            }
            return encryptStr;
        }
        /// <summary>
        /// aes256 解密
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptStr, byte[] key, byte[] iv)
        {
            byte[] toEncryptArray = Convert.FromBase64String(encryptStr);

            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                //rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.KeySize = 256;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(key, iv);

                // Create the streams used for decryption.  
                using (var msDecrypt = new MemoryStream(toEncryptArray))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {

                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream  
                            // and place them in a string.  
                            encryptStr = srDecrypt.ReadToEnd();
                            srDecrypt.Dispose();
                        }
                        csDecrypt.Dispose();
                    }
                    msDecrypt.Dispose();
                }
                rijAlg.Dispose();
            }
            return encryptStr;
        }

        /// <summary>
        /// aes256 加密
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Encrypt(string encryptStr, byte[] key, byte[] iv)
        {
           byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);

            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                //rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.KeySize = 256;

                // Create a decrytor to perform the stream transform.  
                var encryptor = rijAlg.CreateEncryptor(key, iv);

                string  result  = Convert.ToBase64String(encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length));

                rijAlg.Dispose();

                return result;
            }
        }

    }
}
