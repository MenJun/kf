using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Common.Utils;

namespace Common.Filter
{
    public class ControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            System.Exception exception = actionExecutedContext.Exception;

            //string errmsg = exception.Message;
            //while (exception.InnerException != null)
            //{
            //    errmsg += exception.InnerException.Message;
            //    exception = exception.InnerException;
            //}

            var body = ExceptionHelper.FormatExceptionMessage(exception);

            HttpResponseMessage responseMessage = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, body);
            
            actionExecutedContext.Response = responseMessage;
            // new ExceptionActionResult(responseMessage);

            base.OnException(actionExecutedContext);
        }
    }
}
