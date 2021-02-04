using System.Web.Http;
using Api.Model.VO;
using Api.Services.V1;
using Common.Filter;
using Microsoft.Web.Http;
using Unity.Attributes;

namespace Api.Controllers.V1
{
    /// <summary>
    /// 账户验证
    /// </summary>
    [JwtAuthorizationFilter]
    [ApiVersion("1.0")]
    //[RoutePrefix("api/v{version:apiVersion}/account")]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        /// <summary>
        /// Account业务逻辑
        /// </summary>
        [Dependency]
        public AccountService Service
        {
            get;
            set;
        }
        /// <summary>
        /// 验证登录
        /// </summary>
        /// <param name="vo">登录信息</param>
        /// <returns></returns>
        [Route("login")]
        [Transaction]
        [AllowAnonymous]
        [ModelValidation]
        [HttpPost]
        public Response Login([FromBody]LoginVO vo)
        {
            return Service.Login(vo);
        }
        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="token">token</param>
        /// <returns></returns>
        [Route("refresh/token")]
        [AllowAnonymous]
        [HttpGet]
        public Response RefreshToken([FromUri]string token)
        {
            return Service.RefreshToken(token);
        }

        /// <summary>
        /// 生成访客token
        /// </summary>
        /// <returns></returns>
        [Route("guest/token")]
        [AllowAnonymous]
        [HttpGet]
        public Response GenerateGuestToken()
        {
            return Service.GenerateGuestToken();
        }
        /// <summary>
        /// 用户更改密码
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        [Route("pwd/update")]
        [HttpPut]
        [ModelValidation]
        [Transaction]
        public Response ResetPwd([FromBody] PwdVo vo)
        {
            return Service.UpdatePwd(vo);
        }

        /// <summary>
        /// 查询职务权限
        /// </summary>
        /// <param name="id">职务id</param>
        /// <param name="permissions">权限</param>
        /// <returns></returns>
        [Route("permissions/update")]
        [ModelValidation]
        [HttpPost]
        [Transaction]
        public Response UpdatePermissions([FromUri]int id, [FromBody]int[] permissions)
        {
            return Service.UpdatePermissions(id, permissions);
        }
        /// <summary>
        /// 初始化权限
        /// </summary>
        /// <returns></returns>
        [Route("permission/init")]
        [AllowAnonymous]
        [HttpPost]
        public Response InitPermission()
        {
            return Service.InitPermission();
        }
        /// <summary>
        /// 获取职务权限模块
        /// </summary>
        /// <param name="id">职务id</param>
        /// <returns></returns>
        [Route("modules")]
        [HttpGet]
        public Response Modules([FromUri]int id)
        {
            return Service.QueryRoleEnableModules(id);
        }
        /// <summary>
        /// 更新职务权限模块
        /// </summary>
        /// <param name="id">职务id</param>
        /// <param name="modules">职务模块</param>
        /// <returns></returns>
        [Route("modules/update")]
        [ModelValidation]
        [HttpPost]
        public Response UpdateModules([FromUri]int id, [FromBody]int[] modules)
        {
            return Service.UpdateModules(id, modules);
        }
        /// <summary>
        /// 查询职务权限
        /// </summary>
        /// <param name="id">职务id</param>
        /// <returns></returns>
        [Route("permissions")]
        [ModelValidation]
        [HttpGet]
        [Transaction]
        public Response Permissions([FromUri]int id)
        {
            return Service.QueryRolePermissions(id);
        }

        ///// <summary>
        ///// 微信用户注册
        ///// </summary>
        ///// <param name="wxCode"></param>
        ///// <param name="user">微信用户信息</param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //[HttpPost]
        //public IHttpActionResult WxRegister([FromUri]string wxCode,[FromBody]ChannelStaffVO user)
        //{
        //    return Ok(Service.WxRegister(wxCode,user));
        //}
        ///// <summary>
        ///// 微信-小程序登录
        ///// </summary>
        ///// <param name="login">wx.login获取到的用户码</param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //[Route("wxlogin")]
        //[HttpPost]
        //[Transaction]
        //public Response WxLogin(LoginVO login)
        //{
        //    return Service.WxLogin(login);
        //}

        /// <summary>
        /// 微信-小程序获取用户绑定手机号码
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("bindWx")]
        [HttpPost]
        [Transaction]
        public Response BindWx(WxPayloadVO payload)
        {
            return Service.BindWx(payload);
        }

        //[AllowAnonymous]
        //[Route("register")]
        //[HttpPost]
        //[Transaction]
        //public Response Register(WxPayloadVO payload)
        //{
        //    return Service.Register(payload);
        //}

    }
}
