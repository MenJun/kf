using System;
using System.Collections.Generic;
using System.Linq;
using Api.Model.DO;
using Common.Utils;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;

namespace Api.Dao.V1
{
    public class RoleDao
    {

        public int TotalRecords(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                string sqlNO = "";
                if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
                {
                    sqlNO = @"  AND ar.FNAME LIKE '%" + filter["name"].ToString() + "%'";
                }
                string sql = @" SELECT count(1) FROM (select FID,ar.FNAME,FSTOREID,tcl.FNAME FSTORENAME from A_ROLE ar 
                                 LEFT JOIN  T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=ar.FSTOREID   where 1=1  " + sqlNO + " AND tcl.FCHANNELID like :p1) t     ";
                // " + sqldate + sqlNO + " 
                var total = session
                    .CreateSQLQuery(sql)
                    .SetParameter("p1", "%" + filter["channelId"] + "%")
                    .List<int>()
                    .FirstOrDefault();

                return total;
            
        }

        /// <summary>
        /// 职务信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IList<ARoleList> RoleList(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                string sqlNO = "";
                if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
                {
                    sqlNO = @"  AND ar.FNAME LIKE '%" + filter["name"].ToString() + "%'";
                }

                string sql = @" select *  from ( select FID,ar.FNAME,FSTOREID,tcl.FNAME FSTORENAME,ROW_NUMBER() over(order by ar.fid desc) XH from A_ROLE ar 
                                           LEFT JOIN  T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=ar.FSTOREID  where 1=1  " + sqlNO + " AND tcl.FCHANNELID like :p1) t  ";

                //sql += $" order by t.FID desc  offset { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } rows fetch next { Convert.ToInt32(filter["limit"]) } rows only";
                sql += $"where XH > { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } and XH <= { (Convert.ToInt32(filter["page"])) * Convert.ToInt32(filter["limit"]) }";
                IList<ARoleList> activicyLists = session
                    .CreateSQLQuery(sql)
                    .SetParameter("p1", "%" + filter["channelId"] + "%")
                    .SetResultTransformer(Transformers.AliasToBean<ARoleList>())
                    .List<ARoleList>();

                return activicyLists;
            
        }


        /// <summary>
        /// 保存职务信息
        /// </summary>
        /// <param name="aactivity">职务信息</param>
        public void Save(ARole aactivity)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                session.Save(aactivity);
                session.Flush();
            
        }

        /// <summary>
        /// 保存职务信息
        /// </summary>
        /// <param name="aactivity">职务信息</param>
        public void Edit(ARole aactivity)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                session.Update(aactivity);
                session.Flush();
            
        }

        /// <summary>
        /// 删除职务信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int Delete(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                string sql1 = "DELETE FROM ARole  WHERE FID =:p1 ";

                int result = session
                    .CreateQuery(sql1)
                    .SetParameter("p1", id)
                    .ExecuteUpdate();

                return result;
            
        }

        /// <summary>
        /// 获取职务信息
        /// </summary>
        /// <returns></returns>
        public IList<ARole> DetailById(int Id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            
                var aRoles = session
                    .QueryOver<ARole>()
                    .Where(x => x.FID == Id)
                    .List<ARole>();
                return aRoles;
            
        }
    }
}
