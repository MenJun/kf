// *************************************************************
//
// 文件名(File Name)：HttpError
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/15 14:52:26
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

namespace Common.Utils
{
    public class ExceptionHelper
    {
        /// <summary>
        /// 未知异常
        /// </summary>
        public const int UNKNOWN = -10000;
        /// <summary>
        /// JWT验证失败，非法的请求
        /// </summary>
        public const int JWTAUTHERROR = 10001;
        /// <summary>
        /// 请求头不含Token
        /// </summary>
        public const int MISSJWTTOKEN = 10002;
        /// <summary>
        /// Token已过期
        /// </summary>
        public const int JWTTOKENEXPIRED = 10003;
        /// <summary>
        /// 非法的Token
        /// </summary>
        public const int ILLEGALJWTTOKEN = 10004;
        /// <summary>
        /// 参数为空
        /// </summary>
        public const int NULLPARAMETER = 10005;
        /// <summary>
        /// 不合法的参数，未通过模型验证
        /// </summary>
        public const int ILLEGALPARAMETER = 10006;
        /// <summary>
        /// 不合法的参数，系统未知参数
        /// </summary>
        public const int SYSTEMUNKNOWNPARAMETER = 10007;
        /// <summary>
        /// 会话已过期
        /// </summary>
        public const int SESSIONEXPIRED = 10008;
        /// <summary>
        /// 参数签名校验失败-缺少时间戳
        /// </summary>
        public const int MISSTIMESTAMP = 10009;
        /// <summary>
        /// 参数签名校验失败-缺少签名
        /// </summary>
        public const int MISSSIGN = 10010;
        /// <summary>
        /// 参数签名校验失败-非法的签名
        /// </summary>
        public const int ILLEGALSIGN = 10011;
        /// <summary>
        /// 数据库查询结果为NULL
        /// </summary>
        public const int DBNOTFOUND = 30001;
        /// <summary>
        /// SQL语法错误
        /// </summary>
        public const int DBSQLERROR = 30002;
        /// <summary>
        /// SQL连接失败
        /// </summary>
        public const int DBCONNECTIONERROR = 30003;
        /// <summary>
        /// 不合法的参数，未通过数据库验证
        /// </summary>
        public const int DBNOTEXISTS = 30006;
        /// <summary>
        /// 没有操作权限
        /// </summary>
        public const int NOACTIONPERMISSION = 40001;
        /// <summary>
        /// 达摩api接口请求错误
        /// </summary>
        public const int DAMOAPIERROR = 50002;
        /// <summary>
        /// 微信请求失败
        /// </summary>
        public const int WXREQUESTFAIL = 90001;
        /// <summary>
        /// 微信登录失败
        /// </summary>
        public const int WXLOGINFAIL = 90002;
        /// <summary>
        /// 地址解析错误
        /// </summary>
        public const int WXLOCATIONTRANSERROR = 90003;
        /// <summary>
        /// 没有找到地址信息
        /// </summary>
        public const int WXLOCATIONNOTFOUND = 90004;

        public static object FormatExceptionMessage(Exception exception)
        {
            var message = exception.Message;
            var ex = exception;
            while (ex.InnerException != null)
            {
                message += ex.InnerException.Message;
                ex = ex.InnerException;
            }

            var extype = exception.GetType();
            switch (extype.FullName)
            {
                case "NHibernate.Exceptions.GenericADOException":

                    if (message.Contains("有语法错误"))
                    {
                        return new
                        {
                            errcode = DBSQLERROR,
                            errmsg = "SQL中有语法错误"
                        };
                    }
                    else
                    {
                        return new
                        {
                            errcode = UNKNOWN,
                            errmsg = ex.Message
                        };
                    }
                case "System.TypeInitializationException":
                    if (message.Contains("未找到或无法访问服务器"))
                    {
                        return new
                        {
                            errcode = DBCONNECTIONERROR,
                            errmsg = "无法连接SQL SERVER服务器。"
                        };
                    }
                    else
                    {
                        return new
                        {
                            errcode = UNKNOWN,
                            errmsg = ex.Message
                        };
                    }
                default:
                    if (message.Contains("damo api error"))
                    {
                        return new
                        {
                            errcode = DAMOAPIERROR,
                            errmsg = message
                        };
                    }
                    else if (message.Contains("jwt auth error"))
                    {
                        return new
                        {
                            errcode = JWTAUTHERROR,
                            errmsg = "身份认证失败！"
                        };
                    }
                    else if (message.Contains("miss jwt token"))
                    {
                        return new
                        {
                            errcode = MISSJWTTOKEN,
                            //errmsg = "缺少Token！"
                            errmsg = "请先登录！"
                        };
                    }
                    else if (message.Contains("jwt token illegal"))
                    {
                        return new
                        {
                            errcode = ILLEGALJWTTOKEN,
                            errmsg = "非法的Token！"
                        };
                    }
                    else if (message.Contains("jwt token expired"))
                    {
                        return new
                        {
                            errcode = JWTTOKENEXPIRED,
                            errmsg = "会话已过期！"
                        };
                    }
                    else if (message.Contains("session expired"))
                    {
                        return new
                        {
                            errcode = SESSIONEXPIRED,
                            errmsg = "请求已过期！"
                        };
                    }
                    else if (message.Contains("miss timestamp"))
                    {
                        return new
                        {
                            errcode = MISSTIMESTAMP,
                            errmsg = "缺少时间戳！"
                        };
                    }
                    else if (message.Contains("miss sign"))
                    {
                        return new
                        {
                            errcode = MISSSIGN,
                            errmsg = "缺少签名！"
                        };
                    }
                    else if (message.Contains("sign illegal"))
                    {
                        return new
                        {
                            errcode = ILLEGALSIGN,
                            errmsg = "非法的签名！"
                        };
                    }
                    else if (message.Contains("no action permission"))
                    {
                        return new
                        {
                            errcode = NOACTIONPERMISSION,
                            errmsg = "没有操作权限！"
                        };
                    }
                    else
                    {
                        return new
                        {
                            errcode = UNKNOWN,
                            errmsg = ex.Message
                        };
                    }
            }
        }
    }
}
