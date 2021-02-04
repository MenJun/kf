// *************************************************************
//
// 文件名(File Name)：JwtAuthenticationFilter
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 23:01:56
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Api.Model.BO;
using Common.Utils;
using Newtonsoft.Json;

namespace Common.Filter
{
    public class JwtAuthorizationFilterAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
            {
                return;
            }

            AuthenticationHeaderValue authRequest = actionContext.Request.Headers.Authorization;

            if (authRequest == null || authRequest.Scheme != "Bearer")
            {
                var body = ExceptionHelper.FormatExceptionMessage(new System.Exception("jwt auth error"));
                HttpResponseMessage responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.OK, body);
                actionContext.Response = responseMessage;
                return;
            }

            string token = authRequest.Parameter;
            if (string.IsNullOrWhiteSpace(token))
            {
                var body = ExceptionHelper.FormatExceptionMessage(new System.Exception("miss jwt token"));
                HttpResponseMessage responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.OK, body);
                actionContext.Response = responseMessage;
                return;
            }

            try
            {
                JwtPayload payload = JsonConvert.DeserializeObject<JwtPayload>(JwtHelper.VerifyToken(token).ToString());

                if (payload.Exp <= TimeStampHelper.ToTimeStamp(DateTime.Now))
                {
                    var body = ExceptionHelper.FormatExceptionMessage(new System.Exception("jwt token expired"));
                    HttpResponseMessage responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.OK, body);
                    actionContext.Response = responseMessage;
                    return;
                }
                else if (payload.Iss != JwtHelper.JwtIss || payload.Aud != JwtHelper.JwtAud)
                {
                    var body = ExceptionHelper.FormatExceptionMessage(new System.Exception("jwt token illegal"));
                    HttpResponseMessage responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.OK, body);
                    actionContext.Response = responseMessage;
                    return;
                }
                else
                {

                }
            }
            catch (System.Exception)
            {
                var body = ExceptionHelper.FormatExceptionMessage(new System.Exception("jwt token illegal"));
                HttpResponseMessage responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.OK, body);
                actionContext.Response = responseMessage;
                return;
            }

            base.OnAuthorization(actionContext);
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            Contract.Assert(actionContext != null);

            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                       || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}
