using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Model.BO
{
    /// <summary>
    /// Jwr载荷
    /// </summary>
    public class JwtPayload
    {
        /// <summary>
        /// 签发者
        /// </summary>
        public string Iss
        {
            get;
            set;
        }
        /// <summary>
        /// 面向的用户
        /// </summary>
        public string Sub
        {
            get;
            set;
        }
        /// <summary>
        /// 接收者
        /// </summary>
        public string Aud
        {
            get;
            set;
        }
        /// <summary>
        /// 过期时间（Unix时间戳）
        /// </summary>
        public long Exp
        {
            get;
            set;
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long Iat
        {
            get;
            set;
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId
        {
            get;
            set;
        }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get;
            set;
        }
    }
}