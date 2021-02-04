// *************************************************************
//
// 文件名(File Name)：JwtHelper
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 14:48:28
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
using System.Collections.Generic;
using Api.Model.BO;
using Jose;

namespace Common.Utils
{
    public class JwtHelper
    {
        private static readonly string SecretKey = "jMrcze8eaoKrS9xeJrwMyiia3q+WIii3Fkd+i68NRC12vPYTIPQJbiAwR59sekKFwWD/98Wziomy8/5BVygOq5lTa4d2LtVWb/cGLvLP45s3JZkh2w8YshuzPCHaM5rz5pC8AIubuDoT5nvDizwT1QP2AZvNJdFgZtBuHbrBEXk=";
        public static string JwtIss { get; } = "shuziyu.net.2019";
        public static string JwtAud { get; } = "RAYENWEB";

        private static readonly string JwtTyp = "jwt";
        private static readonly string JwtAlg = "HS256";
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        public static string GenerateToken(int userId, string userName, int hours)
        {
            var payload = new JwtPayload
            {
                Aud = JwtAud,
                Iss = JwtIss,
                Exp = TimeStampHelper.ToTimeStamp(DateTime.Now.AddHours(hours)),
                Iat = TimeStampHelper.ToTimeStamp(DateTime.Now),
                UserId = userId,
                UserName = userName
            };

            var headers = new Dictionary<string, object>()
            {
                { "typ", JwtTyp},
                { "alg", JwtAlg}
            };

            return JWT.Encode(payload, SecretKey, JweAlgorithm.PBES2_HS256_A128KW, JweEncryption.A128CBC_HS256, extraHeaders: headers);
        }

        public static object VerifyToken(string token)
        {
            return JWT.Decode(token, SecretKey, JweAlgorithm.PBES2_HS256_A128KW, JweEncryption.A128CBC_HS256);
        }
    }
}
