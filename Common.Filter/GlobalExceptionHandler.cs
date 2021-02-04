// *************************************************************
//
// 文件名(File Name)：ExceptionsHandler
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 20:23:06
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

using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using Common.Utils;

namespace Common.Filter
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            //Type exceptionType = context.Exception.GetType();

            System.Exception exception = context.Exception;

            //string errmsg = exception.Message;
            //while (exception.InnerException != null)
            //{
            //    errmsg += exception.InnerException.Message;
            //    exception = exception.InnerException;
            //}
            var body = ExceptionHelper.FormatExceptionMessage(exception);
            HttpResponseMessage responseMessage = context.Request.CreateResponse(HttpStatusCode.OK, body);
            context.Result = new ExceptionActionResult(responseMessage);
        }
    }
}
