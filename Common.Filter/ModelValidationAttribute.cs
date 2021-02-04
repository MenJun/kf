// *************************************************************
//
// 文件名(File Name)：ModelValidationFilterAttribute
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/17 10:01:37
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Common.Utils;

namespace Common.Filter
{
    /// <summary>
    /// 模型验证
    /// </summary>
    public class ModelValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var value = actionContext.ActionArguments.Select(x => x.Value);

            if (value.Contains(null) ||
                value.Contains(string.Empty))
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.OK, new
                    {
                        errcode = ExceptionHelper.NULLPARAMETER,
                        errmsg = "参数为空"
                    });
            }

            if (!actionContext.ModelState.IsValid)
            {
                string errorMsg = string.Join(";", actionContext.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));

                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.OK, new
                    {
                        errcode = ExceptionHelper.ILLEGALPARAMETER,
                        errmsg = "参数不合法。"+ errorMsg
                    });
            }
        }
    }
}
