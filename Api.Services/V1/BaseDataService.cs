using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Api.Dao.V1;
using Api.Model.DO;
using Api.Model.VO;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Attributes;
using System.Text;
using Api.Services.V1;

namespace Api.Services.V1
{
    public class BaseDataService
    {
        private static readonly object locker = new object();
        private static readonly string UPLOAD_GOODS_PATH = "upload/goods/";

        [Dependency]
        public BaseDataDao DataDao
        {
            get;
            set;
        }


        /// <summary>
        /// 查询所有门店
        /// </summary>
        /// <returns></returns>
        public Response SelectAllStore()
        {
            var stores = DataDao.SelectAllStore();

            return new Response
            {
                Result = stores
            };
        }

        #region 查询门店职务 对应门店
        /// <summary>
        /// 门店职务
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public Response RoleDetail(int storeId)
        {
            var store = "";
            if (storeId != -1)
            {
                store = storeId.ToString();
            }
           var role = DataDao.RoleDetail(store);
    
            return new Response
            {
                Result = role
            };
        }
        #endregion

   
    }
}
