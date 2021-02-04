using System.Collections.Generic;
using System.Linq;
using Api.Dao.V1;
using Api.Model.DO;
using Api.Model.VO;
using Common.Authority.Core;
using Common.Utils;
using Newtonsoft.Json.Linq;
using Unity.Attributes;

namespace Api.Services.V1
{
    public class RoleService
    {
        private static readonly object locker = new object();
        [Dependency]
        public RoleDao Dao
        {
            get;
            set;
        }

        [Dependency]
        public AuthorityService AuthorityService
        {
            get;
            set;
        }

        [Dependency]
        public BaseDataService BaseDataService
        {
            get;
            set;
        }
        /// <summary>
        /// 获取职务信息
        /// </summary>
        /// <returns></returns>
        public Response SelectRole(string filterStr)
        {
            JObject filter = JObject.Parse(filterStr);

            if(filter["channelId"].ToString() == "-1")
            {
                filter["channelId"] = "";
            }
            var totalRecords = Dao.TotalRecords(filter);

            var roleLists = Dao.RoleList(filter);

            return new Response
            {
                Result = new
                {
                    totalRecords,
                    roleLists
                }
            };
        }

        /// <summary>
        /// 职务信息 --新增
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        public Response Save(ARoleVO vo)
        {
            lock (locker)
            {
                ARole aRole = AutoMapper.Mapper.Map<ARole>(vo);
                if (aRole != null)
                {
                    Dao.Save(aRole);

                    return new Response
                    {
                        Result = 1
                    };
                }
                else
                {
                    return new Response
                    {
                        Errcode = ExceptionHelper.DBNOTEXISTS,
                        Errmsg = "参数不合法。",
                        Result = null
                    };
                }
            }
        }

        /// <summary>
        /// 职务信息 --编辑
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        public Response Edit(ARoleVO vo)
        {
            ARole aRole = AutoMapper.Mapper.Map<ARole>(vo);

            if (aRole != null)
            {
                Dao.Edit(aRole);

                return new Response
                {
                    Result = 1
                };
            }
            else
            {
                return new Response
                {
                    Errcode = ExceptionHelper.DBNOTEXISTS,
                    Errmsg = "参数不合法。",
                    Result = null
                };
            }
        }

        /// <summary>
        /// 职务删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response Delete(int id)
        {
            Dao.Delete(id);
            return new Response
            {
                Result = 1
            };
        }

        /// <summary>
        /// 根据Id查询 -获取职务信息
        /// </summary>
        /// <returns></returns>
        public Response SelectById(int Id)
        {
            var aRoles = Dao.DetailById(Id);


            return new Response
            {
                Result = new
                {
                    aRoles
                }
            };
        }

        public Response UpdateRoleStores(int roleId, IList<int> stores)
        {
            AuthorityService.UpdateRoleStores(roleId, stores.ToArray());
            return new Response
            {
                Result = 1
            };
        }

        public Response QueryRoleStores(int roleId)
        {
            var allStores = BaseDataService.SelectAllStore().Result;

            var authStores = AuthorityService.QueryRoleStores(roleId);

            return new Response
            {
                Result = new
                {
                    allStores,
                    authStores
                }
            };
        }
    }
}
