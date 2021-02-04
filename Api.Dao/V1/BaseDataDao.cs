using System;
using System.Collections.Generic;
using System.Linq;
using Api.Model.BO;
using Api.Model.DO;
using Common.Utils;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;

namespace Api.Dao.V1
{
    public class BaseDataDao
    {

       
        /// <summary>
        /// 获取当前职务拥有的模块
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<CommonBO> QueryRoleHasModules(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = "SELECT A.FID,A.FNAME,B.FROLEID AS PARENTID FROM A_PERGROUP A INNER JOIN A_ROLE_PERGROUP B ON A.FID = B.FPERGROUPID WHERE A.FENABLED = 1 AND B.FROLEID = :P1";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("P1", id)
                .SetResultTransformer(Transformers.AliasToBean(typeof(CommonBO)))
                .List<CommonBO>();
            return result;

        }

 
        /// <summary>
        /// 查询所有门店
        /// </summary>
        public IList<dynamic> SelectAllStore()
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = "SELECT A.FCHANNELID,B.FNAME FROM T_ESS_CHANNEL A INNER JOIN T_ESS_CHANNEL_L B ON A.FCHANNELID = B.FCHANNELID WHERE A.FENABLE = 1";
            IList<dynamic> stores = session
                    .CreateSQLQuery(sql)
                    .List<dynamic>();
            return stores;

        }


        /// <summary>
        /// 查询职务权限
        /// </summary>
        /// <param name="roleId"></param>
        public IList<string> QueryRoleHasPermissions(int roleId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = "SELECT FNAME FROM A_ROLE_PERMISSION  A INNER JOIN A_PERMISSION B ON A.FPERID = B.FID WHERE A.FROLEID = :P1";
            IList<string> pers = session
                    .CreateSQLQuery(sql)
                    .SetParameter("P1", roleId)
                    .List<string>();
            return pers;

        }

        /// <summary>
        /// 门店职务
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> RoleDetail(string storeId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"
select * from A_ROLE where 1=(CASE WHEN EXISTS (select fid from A_ROLE where fstoreid=:p1 ) THEN 0 ELSE 1 END) OR fstoreid = :p1";

            var list = session
                    .CreateSQLQuery(sql)
                    .SetParameter("p1",storeId )
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();

            return list;

        }
      
    }
}
