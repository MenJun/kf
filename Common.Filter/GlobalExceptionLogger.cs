// *************************************************************
//
// 文件名(File Name)：ExceptionLogger
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 20:32:12
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

using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Tracing;

namespace Common.Filter
{
    public class GlobalExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new LogTraceWriter());

            ITraceWriter trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();

            //Type exceptionType = context.Exception.GetType();

            trace.Error(
                context.Request,
                 context.Exception.GetType().Name,
                 context.Exception
            );
        }
    }
}
