// *************************************************************
//
// 文件名(File Name)：LogTraceWriter
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 13:26:31
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
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Http.Tracing;
using Newtonsoft.Json;
using NLog;

namespace Common.Filter
{
    public class LogTraceWriter: ITraceWriter
    {
        //日志写入
        private static readonly Logger AppLogger = LogManager.GetCurrentClassLogger();
        private static readonly Lazy<Dictionary<TraceLevel, Action<string>>> LoggingMap = new Lazy<Dictionary<TraceLevel, Action<string>>>(() => new Dictionary<TraceLevel, Action<string>>
        {
            {TraceLevel.Info,AppLogger.Info },
            {TraceLevel.Debug,AppLogger.Debug },
            {TraceLevel.Error,AppLogger.Error },
            {TraceLevel.Fatal,AppLogger.Fatal },
            {TraceLevel.Warn,AppLogger.Warn }
        });


        private Dictionary<TraceLevel, Action<string>> Logger
        {
            get { return LoggingMap.Value; }
        }
        /// <summary>
        /// 跟踪编写器接口实现
        /// </summary>
        /// <param name="request"></param>
        /// <param name="category"></param>
        /// <param name="level"></param>
        /// <param name="traceAction"></param>
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (level != TraceLevel.Off)//未禁用日志跟踪
            {
                if (traceAction != null && traceAction.Target != null)
                {
                    category = category + Environment.NewLine + "请求参数 : " + JsonConvert.SerializeObject(traceAction.Target);
                }
                var record = new TraceRecord(request, category, level);
                traceAction?.Invoke(record);
                //  traceAction?.Invoke(record);
                Log(record);
            }
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="record"></param>
        private void Log(TraceRecord record)
        {
            var message = new StringBuilder();

            /**************************运行日志****************************/

            //if (!string.IsNullOrWhiteSpace(record.Message))
            //{
            //    message.Append("").Append(record.Message + Environment.NewLine);
            //}

            if (record.Request != null)
            {
                if (record.Request.Method != null)
                {
                    message.Append("请求类型 : " + record.Request.Method + Environment.NewLine);
                }

                if (record.Request.RequestUri != null)
                {
                    message.Append("").Append("请求地址 : " + record.Request.RequestUri + Environment.NewLine);
                }

                if (record.Request.Headers != null && record.Request.Headers.Contains("Token") && record.Request.Headers.GetValues("Token") != null && record.Request.Headers.GetValues("Token").FirstOrDefault() != null)
                {
                    message.Append("").Append("Token : " + record.Request.Headers.GetValues("Token").FirstOrDefault() + Environment.NewLine);
                }

                var ipRecord = GetClientIp(record.Request);
                if (ipRecord != null)
                {
                    message.Append("").Append("IP地址 : " + GetClientIp(record.Request) + Environment.NewLine);
                }
            }

            if (!string.IsNullOrWhiteSpace(record.Category) && !record.Category.Contains("exception"))
            {
                message.Append("").Append(record.Category);
            }

            if (!string.IsNullOrWhiteSpace(record.Operator))
            {
                message.Append(" ").Append(record.Operator).Append(" ").Append(record.Operation);
            }

            //***************************异常日志***********************************//
            if (record.Exception != null && !string.IsNullOrWhiteSpace(record.Exception.GetBaseException().Message))
            {
                var exceptionType = record.Exception.GetType();

                message.Append("").Append("异常类型 : " + exceptionType.ToString());
                message.Append(Environment.NewLine);

                System.Exception exception = record.Exception;
                string errmsg = exception.Message;
                while(exception.InnerException != null)
                {
                    errmsg += exception.InnerException.Message;
                    exception = exception.InnerException;
                }
                message.Append("").Append("异常信息 : " + errmsg);
                
                message.Append(Environment.NewLine);
                message.Append("").Append("堆栈信息 : " + record.Exception.StackTrace);
                message.Append(Environment.NewLine);
            }

            //日志写入本地文件
            Logger[record.Level](Convert.ToString(message) + Environment.NewLine);
        }

        private string GetClientIp(HttpRequestMessage request = null)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }
    }
}
