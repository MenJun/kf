using System.Collections.Generic;
using System.Web.Http;
using Api.Model.VO;
using Api.Services.V1;
using Common.Filter;
using Microsoft.Web.Http;
using Unity.Attributes;

namespace Api.Controllers.V1
{
    /// <summary>
    /// 人员职务
    /// </summary>
    [JwtAuthorizationFilter]
    [ApiVersion("1.0")]
    [RoutePrefix("api/role")]
    public class RoleController : ApiController
    {
        /// <summary>
        /// Activity业务逻辑
        /// </summary>
        [Dependency]
        public RoleService Service
        {
            get;
            set;
        }

        /// <summary>
        /// 获取职务信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[AllowAnonymousAttribute]
        [Transaction]
        [Route("list")]
        public Response RoleList([FromUri]string filterStr)
        {
            return Service.SelectRole(filterStr);
        }


        /// <summary>
        /// 职务 --新增
        /// </summary>
        /// <param name="roleVO">职务信息</param>
        /// <returns></returns>
        [HttpPost]
        [Transaction]
        [Route("add")]
        public Response Save([FromBody]ARoleVO roleVO)
        {
            Response result = Service.Save(roleVO);

            return result;
        }

        /// <summary>
        /// 职务 --编辑
        /// </summary>
        /// <param name="roleVO">职务信息</param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [Route("edit")]
        public Response Edit([FromBody]ARoleVO roleVO)
        {
            Response result = Service.Edit(roleVO);

            return result;
        }

        /// <summary>
        /// 职务--删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Transaction]
        [Route("delete")]
        public Response Delete([FromUri]int id)
        {
            return Service.Delete(id);
        }



        /// <summary>
        /// 根据Id查询 -获取职务信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{Id}")]
        [Transaction]
        public Response SelectById(int Id)
        {
            return Service.SelectById(Id);
        }

        /// <summary>
        /// 根据Id查询 -获取职务信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("authstores")]
        public Response AuthRoleStores([FromUri]int roleId, [FromBody] IList<int> stores)
        {
            return Service.UpdateRoleStores(roleId, stores);
        }
        [HttpGet]
        [Transaction]
        [Route("queryauthstores")]
        public Response QueryRoleStores([FromUri]int roleId)
        {
            return Service.QueryRoleStores(roleId);
        }
    }
}
