// *************************************************************
//
// 文件名(File Name)：LogFilterAttribute
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 13:31:04
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
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Tracing;

namespace Common.Filter
{
    public class LogFilterAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new LogTraceWriter());
            var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            trace.Info(actionContext.Request, "请求控制器 : " + actionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "请求操作 : " + actionContext.ActionDescriptor.ActionName, "JSON", actionContext.ActionArguments);
        }
    }
}
