// *************************************************************
//
// 文件名(File Name)：ExceptionResponseMessage
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 21:12:00
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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Common.Filter
{
    /// <summary>
    /// 格式化相应消息
    /// </summary>
    public class ExceptionActionResult : IHttpActionResult
    {
        private readonly HttpResponseMessage _responseMessage;
        public ExceptionActionResult(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseMessage);
        }
    }
}
