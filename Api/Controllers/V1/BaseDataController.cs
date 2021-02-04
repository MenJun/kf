using System.Collections.Generic;
using System.Web.Http;
using Api.Model.DO;
using Api.Model.VO;
using Api.Services.V1;
using Common.Filter;
using Microsoft.Web.Http;
using Unity.Attributes;

namespace Api.Controllers.V1
{

    /// <summary>
    /// 基础资料
    /// </summary>
    [JwtAuthorizationFilter]
    [ApiVersion("1.0")]
    [RoutePrefix("api/basedata")]
    public class BaseDataController : ApiController
    {
        [Dependency]
        public BaseDataService DataService
        {
            get;
            set;
        }

        #region 查询门店职务 对应门店
        /// <summary>
        /// 门店职务
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("role")]
        public Response RoleDetail(int storeId)
        {
            return DataService.RoleDetail(storeId);
        }
        #endregion
        /// <summary>
        /// 查询所有门店
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("stores")]
        public Response Stores()
        {
            return DataService.SelectAllStore();
        }



    }
}
