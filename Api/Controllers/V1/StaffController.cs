using System.Web.Http;
using Api.Model.DO;
using Api.Model.VO;
using Api.Services.V1;
using Common.Filter;
using Common.Utils;
using Microsoft.Web.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;
using Unity.Attributes;
using System;
using Common.Authority.Entity;
using System.Linq;

namespace Api.Controllers.V1
{
    /// <summary>
    /// 门店职员
    /// </summary>
    [JwtAuthorizationFilter]
    [ApiVersion("1.0")]
    [RoutePrefix("api/staff")]
    public class StaffController : ApiController
    {
        /// <summary>
        /// 业务逻辑
        /// </summary>
        [Dependency]
        public StaffService Service
        {
            get;
            set;
        }

        /// <summary>
        /// 获取门店职员信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("list")]
        public Response StaffList([FromUri]string filterStr)
        {
            return Service.SelectStaff(filterStr);
        }
        [HttpGet]
        [Transaction]
        [Route("listname")]
        public Response StaffListQuery([FromUri]string filterStr)
        {
            return Service.SelectStaffQuery(filterStr);
        }
        /// <summary>
        /// 根据Id查询 -获取门店职员信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("{Id}")]
        public Response SelectById(int Id)
        {
            return Service.SelectById(Id);
        }

        /// <summary>
        /// 门店职员 --新增
        /// </summary>
        /// <param name="channelStaff">订单信息</param>
        /// <returns></returns>
        [HttpPost]
        [ModelValidation]
        [Transaction]
        [Route("add")]
        [AllowAnonymous]
        public Response Save([FromBody]ChannelStaffVO channelStaff)
        {
            Response result = Service.Save(channelStaff);

            return result;
        }

        /// <summary>
        /// 门店职员 --编辑
        /// </summary>
        /// <param name="channelStaff">订单信息</param>
        /// <returns></returns>
        [HttpPut]
        //[ModelValidation]
        [Transaction(System.Data.IsolationLevel.ReadUncommitted)]
        [Route("edit")]
        [AllowAnonymous]
        public Response Edit([FromBody]ChannelStaffVO channelStaff)
        {
            Response result = Service.Edit(channelStaff);

            return result;
        }

        /// <summary>
        /// 门店职员--删除
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
        /// 用户查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("SelectUser")]
        public Response Response([FromUri]string filterStr)
        {
            JObject filter = JObject.Parse(filterStr);
            return Service.SelectUser(filter);
          
        }

        /// <summary>
        /// 单个用户查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("selectEdit")]
        public Response Responses([FromUri]int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT ROW_NUMBER() over(order by dbo.T_ESS_CHANNELSTAFF.FID desc) XH,T_ESS_CHANNELSTAFF.FID, T_ESS_CHANNELSTAFF.KHNAME, T_ESS_CHANNELSTAFF.FMOBILE, 
                T_ESS_CHANNELSTAFF_L.FJOB, T_ESS_CHANNELSTAFF_L.FROLEID, T_ESS_CHANNELSTAFF.FENABLE, 
                T_ESS_CHANNELSTAFF.FQQ, T_ESS_CHANNELSTAFF.FTELE, A_ROLE.FPERMISSIONS, 
                T_ESS_CHANNELSTAFF_1.FID AS KFFID
                FROM      T_ESS_CHANNELSTAFF T_ESS_CHANNELSTAFF_1 INNER JOIN
                T_ESS_CHANNELSTAFF_RelationShip ON
                T_ESS_CHANNELSTAFF_1.FID = T_ESS_CHANNELSTAFF_RelationShip.StaffID RIGHT OUTER JOIN
                T_ESS_CHANNELSTAFF INNER JOIN
                T_ESS_CHANNELSTAFF_L ON T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_L.FID INNER JOIN
                A_ROLE ON T_ESS_CHANNELSTAFF_L.FROLEID = A_ROLE.FID ON
                T_ESS_CHANNELSTAFF_RelationShip.CustomerID = T_ESS_CHANNELSTAFF.FID
                WHERE(T_ESS_CHANNELSTAFF.FID = :p1)";

            
            IList<dynamic> activicyLists = session.CreateSQLQuery(sql)
                    .SetParameter("p1", id)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();

            return new Response
            {
                Result = activicyLists,
            };

        }
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("select")]
        public Response Responsess()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   dbo.A_ROLE.* FROM      dbo.A_ROLE";
            IList<dynamic> activicyLists = session.CreateSQLQuery(sql)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();

            return new Response
            {
                Result = activicyLists,
            };
        }

        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("selectkf")]
        public Response ResponsessKF()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   cs.FID, cs.KHNAME, cs.FCREATEDATE, cs.ISNEW, cs.FMOBILE, cs.FTELE, cs.FQQ, cs.FWECHAT, cs.FENABLE
                        FROM      dbo.T_ESS_CHANNELSTAFF AS cs INNER JOIN
                                            (SELECT   dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME
                                             FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                                                             dbo.T_ESS_CHANNELSTAFF_L ON 
                                                             dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_L.FPKID INNER JOIN
                                                             dbo.A_ROLE ON dbo.T_ESS_CHANNELSTAFF_L.FROLEID = dbo.A_ROLE.FID
                                             WHERE   (dbo.A_ROLE.FPERMISSIONS = '1')) AS derivedtbl_1 ON cs.FID = derivedtbl_1.FID";
            IList<dynamic> activicyLists = session.CreateSQLQuery(sql)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();

            return new Response
            {
                Result = activicyLists,
            };
        }

        /// <summary>
        /// 更改客服批量修改
        /// </summary>
        /// <param name="id">被选中客服fid</param>
        /// <param name="FID">用户fid</param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("DatchEditKF")]
        public Response DatchEditKF([FromUri]int id, [FromUri]string filterStr)
        {
            Service.DatchEditKF(id, filterStr);


            return new Response
            {
                Result = new
                {
                    id
                }
            };
        }

        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("Edits")]
        public Response Responsesss([FromUri]int id, [FromUri] int FID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var ROLE = session.QueryOver<ARole>()
                .And(x => x.FID == id)
                .List()
                .FirstOrDefault();
            var sql = @"update T_ESS_CHANNELSTAFF_L set FJOB='" + ROLE.FNAME + "',FROLEID=" + ROLE.FID + " where FPKID=" + FID;
            var CHANNELSTAFF = session.CreateSQLQuery(sql)
                .ExecuteUpdate();
            sql = @"delete from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = '" + FID + "'";
            CHANNELSTAFF = session.CreateSQLQuery(sql)
                .ExecuteUpdate();
            return new Response
            {
                Result = 1,
            };
        }

        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("SelectFPERMISSIONS")]
        public Response ResponsesssS([FromUri]int id, [FromUri] int FID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   dbo.A_ROLE.* FROM dbo.A_ROLE where FID=" + id;
            IList<dynamic> activicyLists = session.CreateSQLQuery(sql)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();
            var sql1 = @"SELECT ROW_NUMBER() over(order by dbo.T_ESS_CHANNELSTAFF.FID desc) XH, 
                dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, dbo.T_ESS_CHANNELSTAFF.FMOBILE, 
                dbo.T_ESS_CHANNELSTAFF_L.FJOB, dbo.T_ESS_CHANNELSTAFF_L.FROLEID, dbo.T_ESS_CHANNELSTAFF.FENABLE, 
                dbo.T_ESS_CHANNELSTAFF.FQQ, dbo.T_ESS_CHANNELSTAFF.FTELE, dbo.A_ROLE.FPERMISSIONS
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_L ON dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_L.FID INNER JOIN
                dbo.A_ROLE ON dbo.T_ESS_CHANNELSTAFF_L.FROLEID = dbo.A_ROLE.FID  where   dbo.T_ESS_CHANNELSTAFF.FID =" + FID;
            IList<dynamic> activicyLists1 = session.CreateSQLQuery(sql1)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();
            return new Response
            {
                Result = new
                {
                    activicyLists,
                    activicyLists1
                }
            };
        }

        /// <summary>
        /// 添加客服
        /// </summary>
        /// <param name="id">被选中客服fid</param>
        /// <param name="FID">用户fid</param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("InsertFPERMISSIONSKF")]
        public Response ResponsesssKFs([FromUri]int id, [FromUri] int FID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var sql = @"INSERT INTO T_ESS_CHANNELSTAFF_RelationShip(StaffID,CustomerID,CreateTime) VALUES(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()))";
            IList<dynamic> activicyLists1 = session.CreateSQLQuery(sql)
                    .SetParameter("p1", id)
                    .SetParameter("p2", FID)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();
            return new Response
            {
                Result = new
                {
                    activicyLists1
                }
            };
        }

        /// <summary>
        /// 更改客服
        /// </summary>
        /// <param name="id">被选中客服fid</param>
        /// <param name="FID">用户fid</param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("SelectFPERMISSIONSKF")]
        public Response ResponsesssKF([FromUri]int id, [FromUri] int FID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var sql = @"update T_ESS_CHANNELSTAFF_RelationShip set StaffID=:p1 where CustomerID=:p2";
            IList<dynamic> activicyLists1 = session.CreateSQLQuery(sql)
                    .SetParameter("p1", id)
                    .SetParameter("p2", FID)
                    .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    .List<dynamic>();
            return new Response
            {
                Result = new
                {
                    activicyLists1
                }
            };
        }



        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        [Route("Relation")]
        public Response Relation([FromUri] int FID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   derivedtbl_1.FNAME, derivedtbl_2.KHNAME, derivedtbl_2.createTime, derivedtbl_1.FPKID, 
                derivedtbl_2.FWXOPENID AS 外部人员openid, derivedtbl_1.FWXOPENID AS 内部人员openid
                FROM      T_ESS_CHANNELSTAFF_RelationShip INNER JOIN
                    (SELECT   T_ESS_CHANNELSTAFF_L.FPKID, T_ESS_CHANNELSTAFF_L.FNAME, A_ROLE.FPERMISSIONS,
                                     T_ESS_CHANNELSTAFF.FWXOPENID
                     FROM      A_ROLE INNER JOIN
                                     T_ESS_CHANNELSTAFF_L ON A_ROLE.FID = T_ESS_CHANNELSTAFF_L.FROLEID INNER JOIN
                                     T_ESS_CHANNELSTAFF ON T_ESS_CHANNELSTAFF_L.FPKID = T_ESS_CHANNELSTAFF.FID
                     WHERE(A_ROLE.FPERMISSIONS = '1')) derivedtbl_1 ON T_ESS_CHANNELSTAFF_RelationShip.StaffID = derivedtbl_1.FPKID INNER JOIN
                 (SELECT T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME,
                                  T_ESS_CHANNELSTAFF_1.FWXOPENID, news.createTime
                  FROM      T_ESS_CHANNELSTAFF T_ESS_CHANNELSTAFF_1 INNER JOIN

                                      (SELECT FromUserName, MAX(Id) AS id, MAX(CreateTime) AS createTime
                                       FROM      T_CUS_SERVER_MSG
                                       GROUP BY FromUserName) news ON T_ESS_CHANNELSTAFF_1.FWXOPENID = news.FromUserName) 
                derivedtbl_2 ON T_ESS_CHANNELSTAFF_RelationShip.CustomerID = derivedtbl_2.FID
                WHERE(derivedtbl_1.FPKID = :p1)";
            var Relation = session.CreateSQLQuery(sql).SetParameter("p1", FID)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return new Response
            {
                Result = Relation
            };

        }
    }
}
