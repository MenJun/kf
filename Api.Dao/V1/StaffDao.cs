using System;
using System.Collections.Generic;
using System.Linq;
using Api.Model.DO;
using Common.Utils;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace Api.Dao.V1
{
    public class StaffDao
    {
        public int TotalRecords(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sqlNO = "";
            if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
            {
                sqlNO = $"  AND (cs.KHNAME LIKE '%{filter["name"].ToString()}%' OR cs.FMOBILE LIKE '%{filter["name"].ToString()}%'  )  ";
            }

            string sql = @" SELECT count(1) FROM (   SELECT cs.FID, csl.FNAME,cs.FMOBILE ,FTELE,FQQ,FWECHAT,FENABLE,csl.FJOB,tcl.FCHANNELID,TCL.FNAME FCHANNELNAME FROM T_ESS_CHANNELSTAFF cs 
                                     LEFT OUTER JOIN T_ESS_CHANNELSTAFF_L csl ON (cs.FID = csl.FID AND csl.FLOCALEID = 2052) 
                                     LEFT OUTER JOIN T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=CS.FCHANNELID AND TCL.FLOCALEID=2052  where 1=1  " + sqlNO + "  AND tcl.FCHANNELID=27) t     ";

            var total = session
                .CreateSQLQuery(sql)
                .List<int>()
                .FirstOrDefault();

            return total;

        }

        /// <summary>
        /// 门店职员信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IList<dynamic> StaffList(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sqlNO = "";
            if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
            {
                sqlNO = $"  AND (cs.KHNAME LIKE '%{filter["name"].ToString()}%' OR cs.FMOBILE LIKE '%{filter["name"].ToString()}%'  )  ";

            }
            string sql = @" select * from (    SELECT cs.FID,cs.KHNAME,cs.FCREATEDATE,csl.FNAME,cs.ISNEW, cs.FMOBILE,FTELE,FQQ,FWECHAT,FENABLE,csl.FJOB,tcl.FCHANNELID,TCL.FNAME FCHANNELNAME,ROW_NUMBER() over(order by cs.FID desc) XH FROM T_ESS_CHANNELSTAFF cs 
                                     LEFT OUTER JOIN T_ESS_CHANNELSTAFF_L csl ON (cs.FID = csl.FID AND csl.FLOCALEID = 2052) 
                                     LEFT OUTER JOIN T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=CS.FCHANNELID AND TCL.FLOCALEID=2052 where 1=1 " + sqlNO + " ) t  ";

            //sql += $"  order by t.FID desc offset { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } rows fetch next { Convert.ToInt32(filter["limit"]) } rows only";
            sql += $"where XH > { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } and XH <= { (Convert.ToInt32(filter["page"])) * Convert.ToInt32(filter["limit"]) }";

            var list = session
                .CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();

            return list;
        }

        public int QueryFnameTotalRecords(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sqlNO = "";
            if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
            {
                sqlNO = $"  AND (cs.KHNAME LIKE '%{filter["name"].ToString()}%' OR cs.FMOBILE LIKE '%{filter["name"].ToString()}%'  )  ";
            }

            string sql = @" SELECT count(1) FROM (   SELECT cs.FID, csl.FNAME,cs.FMOBILE ,FTELE,FQQ,FWECHAT,FENABLE,csl.FJOB,tcl.FCHANNELID,TCL.FNAME FCHANNELNAME FROM T_ESS_CHANNELSTAFF cs 
                                     LEFT OUTER JOIN T_ESS_CHANNELSTAFF_L csl ON (cs.FID = csl.FID AND csl.FLOCALEID = 2052) 
                                     LEFT OUTER JOIN T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=CS.FCHANNELID AND TCL.FLOCALEID=2052  where 1=1  " + sqlNO + "  AND tcl.FCHANNELID like :p1  AND tcl.FCHANNELID<>27) t     ";

            var total = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", "%" + filter["channelId"] + "%")
                .List<int>()
                .FirstOrDefault();

            return total;

        }
        public IList<dynamic> StaffListQueryFname(JObject filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sqlNO = "";
            if (!string.IsNullOrWhiteSpace(filter["name"].ToString()))
            {
                sqlNO = $"  AND (csL.FNAME LIKE '%{filter["name"].ToString()}%' OR cs.FMOBILE LIKE '%{filter["name"].ToString()}%'  )  ";

            }
            string sql = @" select * from (    SELECT cs.FID,cs.KHNAME,csl.FNAME, cs.FMOBILE,FTELE,FQQ,FWECHAT,FENABLE,csl.FJOB,tcl.FCHANNELID,TCL.FNAME FCHANNELNAME,ROW_NUMBER() over(order by cs.FID desc) XH FROM T_ESS_CHANNELSTAFF cs 
                                     LEFT OUTER JOIN T_ESS_CHANNELSTAFF_L csl ON (cs.FID = csl.FID AND csl.FLOCALEID = 2052) 
                                     LEFT OUTER JOIN T_ESS_CHANNEL_L tcl ON tcl.FCHANNELID=CS.FCHANNELID AND TCL.FLOCALEID=2052 where 1=1 " + sqlNO + "  AND tcl.FCHANNELID like :p1  AND tcl.FCHANNELID <> 27) t ";

            //sql += $"  order by t.FID desc offset { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } rows fetch next { Convert.ToInt32(filter["limit"]) } rows only";
            sql += $"where XH > { (Convert.ToInt32(filter["page"]) - 1) * Convert.ToInt32(filter["limit"]) } and XH <= { (Convert.ToInt32(filter["page"])) * Convert.ToInt32(filter["limit"]) }";

            var list = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", "%" + filter["channelId"] + "%")
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();

            return list;
        }

        /// <summary>
        /// 获取门店职员信息
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> DetailById(int Id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @" select * from T_ESS_CHANNELSTAFF WHERE FID=:p1 ";

            var list = session
                .CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .SetParameter("p1", Id)
                .List<dynamic>();
            return list;

        }

        /// <summary>
        /// 查询系统登录用户
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public dynamic QuerySystemUserByPhoneNumber(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

       //     string sql = @"SELECT  A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
       //             FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
       //            LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   //LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
       //            LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID,'') = ''  AND  A.FMOBILE = :P1";

            string sql = @"SELECT  A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   left join A_ROLE R on R.FID = B.FROLEID
				   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND R.FNAME='客服'   AND  A.FMOBILE = :P1";
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;

        }

        public dynamic QueryWxappUserByPhoneNumber(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            //     string sql = @"SELECT  A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
            //             FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
            //            LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
            //LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
            //            LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID,'') <> '' AND  A.FMOBILE = :P1";
            string sql = @"SELECT  A.FID,A.FWXOPENID,  A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME,A.XCXOPENID
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   left join A_ROLE R on R.FID = B.FROLEID
				   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE  A.FMOBILE = :P1";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;

        }

        public dynamic QueryWxappUserByXCXOpenid(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT * FROM dbo.T_ESS_CHANNELSTAFF WHERE FMOBILE = :P1  AND XCXOPENID IS NOT NULL";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;
        }
        public dynamic QueryWxappUserByFwxOpenid(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT * FROM dbo.T_ESS_CHANNELSTAFF WHERE FMOBILE = :P1  AND FWXOPENID IS NOT NULL";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;
        }

        public void QueryWxappUserEditXCXOpenid(string phoneNumber, string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"update T_ESS_CHANNELSTAFF set XCXOPENID = :P2 where FMOBILE = :P1";

            session .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetParameter("P2", wxopenid)
                .ExecuteUpdate();
        }
        public void QueryWxappUserEditFwxOpenid(string phoneNumber, string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"update T_ESS_CHANNELSTAFF set FWXOPENID = :P2 where FMOBILE = :P1";

            session.CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetParameter("P2", wxopenid)
                .ExecuteUpdate();
        }

        /// <summary>
        /// 查找出所有的客服按聊天时间倒叙
        /// </summary>
        /// <param name="FWXOPENID"></param>
        public IList<dynamic> ZXKH_SelectStaff()
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            //string sql = @"SELECT   TOP (100) PERCENT a_1.FID, m1.CreateTime, m1.XCXFromOpenId
            //     FROM      (SELECT   a.FID, a.KHNAME, b.PICTURE, a.FWXOPENID
            //     FROM      dbo.T_ESS_CHANNELSTAFF AS a LEFT OUTER JOIN
            //                     dbo.T_ESS_CHANNELSTAFF_AVATAR AS b ON a.FID = b.STAFFID LEFT OUTER JOIN
            //                     dbo.T_ESS_CHANNELSTAFF_L AS c ON c.FID = a.FID LEFT OUTER JOIN
            //                     dbo.A_ROLE AS d ON d.FID = c.FROLEID
            //     WHERE   (d.FPERMISSIONS = 1) AND (a.FWXOPENID IN
            //                         (SELECT   FromUserName
            //                          FROM      dbo.T_CUS_SERVER_MSG
            //                          WHERE   (MsgId > 0)
            //                          GROUP BY FromUserName))) AS a_1 LEFT OUTER JOIN
            //        (SELECT   FromUserName, MAX(Id) AS id, MAX(CreateTime) AS createTime
            //         FROM      dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
            //         GROUP BY FromUserName) AS m ON m.FromUserName = a_1.FWXOPENID LEFT OUTER JOIN
            //    dbo.T_CUS_SERVER_MSG AS m1 ON m1.Id = m.id
            //    ORDER BY m1.CreateTime DESC";
            string sql = @"select * from (
                   SELECT  ROW_NUMBER()   
                    over   
                (PARTITION By dbo.T_ESS_CHANNELSTAFF.FID order by dbo.T_CUS_SERVER_MSG.Id desc) as rowId,dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID, dbo.A_ROLE.FPERMISSIONS, dbo.T_CUS_SERVER_MSG.Id, 
                dbo.T_CUS_SERVER_MSG.[Content]
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_L ON dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_L.FPKID INNER JOIN
                dbo.A_ROLE ON dbo.T_ESS_CHANNELSTAFF_L.FROLEID = dbo.A_ROLE.FID INNER JOIN
                dbo.T_CUS_SERVER_MSG ON 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID = dbo.T_CUS_SERVER_MSG.XCXFromOpenId
                WHERE   (dbo.A_ROLE.FPERMISSIONS = N'1')
                ) t
                where rowid <= 1";
            return session.CreateSQLQuery(sql)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }


        /// <summary>
        /// 客户与客服是否有关联
        /// </summary>
        /// <param name="FWXOPENID">用户openid</param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_Staff_Customers(string FWXOPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"select ShipID,StaffID,CustomerID from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = (select fid from T_ESS_CHANNELSTAFF where XCXOPENID=:p1)";
            return session.CreateSQLQuery(sql)
               .SetParameter("p1", FWXOPENID)
               .SetResultTransformer(new AliasToEntityMapResultTransformer())
               .List<dynamic>();
        }
        public void ZXKH_Delete_Customers(string FWXOPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"delete from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = (select fid from T_ESS_CHANNELSTAFF where XCXOPENID=:p1)";
            session.CreateSQLQuery(sql)
            .SetParameter("p1", FWXOPENID)
            .ExecuteUpdate();
        }
        /// <summary>
        /// 添加用户和管理者的关联
        /// </summary>
        /// <param name="FWXOPENID">用户id</param>
        public void ZXKH_AddShip(string FWXOPENID, long FPKID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"insert into T_ESS_CHANNELSTAFF_RelationShip values(:p2,(select FID from T_ESS_CHANNELSTAFF WHERE XCXOPENID = :p1), DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()))";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", FWXOPENID)
                   .SetParameter("p2", FPKID)
                   .ExecuteUpdate();
        }


        public dynamic QueryCustomerServiceStaffByPhoneNumber(string phoneNumber, int roleId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            //     string sql = @"SELECT  A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
            //             FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
            //            LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
            //LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
            //            LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID,'') <> '' AND  A.FMOBILE = :P1 AND b.FROLEID = :P2";
            string sql = @"SELECT  A.FID, A.FWXOPENID,  A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID,'') <> '' AND  A.FMOBILE = :P1 AND b.FROLEID = :P2";
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetParameter("P2", roleId)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;

        }
        public dynamic QueryWxappUserByPhoneNumbers(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT  A.FID,A.FWXOPENID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 0 AND ISNULL(A.FWXOPENID,'') <> '' AND  A.FMOBILE = :P1";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;

        }
        public ARole QueryCustomerServiceRole()
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            return session
                 .QueryOver<ARole>()
                 .And(x => x.FNAME == "客服")
                 .List()
                 .FirstOrDefault();
        }
        /// <summary>
        /// 根据电话号码查询门店职员信息
        /// </summary>
        /// <returns></returns>
        public dynamic QueryAllUserByPhoneNumber(string phoneNumber)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT  A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID 
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID 
				   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON (TTL.FTYPEID=L.FCHANNELTYPEID AND TTL.FLOCALEID=2052 )  WHERE A.FENABLE = 1 AND  A.FMOBILE = :P1";
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", phoneNumber)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;

        }

        /// <summary>
        /// 根据wxopenid查询客户信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <returns></returns>
        public ESSChannelStaff QueryCustomerByWxopenid(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var customer = session
                .QueryOver<ESSChannelStaff>()
                .And(x => x.XCXOPENID == wxopenid)
                .List()
                .FirstOrDefault();
            return customer;
        }

        public long QueryCustomerByWxopenid(string wxopenid, int roleId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = "select a.FID from T_ESS_CHANNELSTAFF a LEFT JOIN T_ESS_CHANNELSTAFF_L b on b.FID = a.FID where a.FWXOPENID = :p1 AND b.FROLEID = :p2";
            var customer = session
                .CreateSQLQuery(sql)
                .SetParameter("p1",wxopenid)
                .SetParameter("p2", roleId)
                .List<long>()
                .FirstOrDefault();
            return customer;
        }

        /// <summary>
        /// 查询客户头像
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ESSChannelStaffAvatar QueryCustomerAvatar(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var avatar = session
                .QueryOver<ESSChannelStaffAvatar>()
                .And(x => x.StaffId == id)
                .List()
                .FirstOrDefault();
            return avatar;
        }

        /// <summary>
        /// 获取门店职员副表信息
        /// </summary>
        /// <returns></returns>
        public IList<ESSChannelStaff_L> DetailEntryById(int Id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var esschannelStaffs = session
                .QueryOver<ESSChannelStaff_L>()
                .Where(x => x.FID == Id)
                .List<ESSChannelStaff_L>();
            return esschannelStaffs;

        }

        /// <summary>
        /// 保存门店职员信息
        /// </summary>
        /// <param name="staff">门店职员信息</param>
        public void Save(ESSChannelStaff staff)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            session.Save(staff);
            session.Flush();

        }

        public void SaveAvatar(ESSChannelStaffAvatar avatar)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            session.Save(avatar);
        }

        /// <summary>
        /// 更改门店职员信息
        /// </summary>
        /// <param name="staffL">门店职员信息</param>
        public void Edit(ESSChannelStaff staff)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            session.SaveOrUpdate(staff);
            session.Flush();

        }

        /// <summary>
        /// 保存门店职员副表信息
        /// </summary>
        /// <param name="staff">门店职员副表信息</param>
        public void SaveEntry(ESSChannelStaff_L staffL)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            session.Save(staffL);
            session.Flush();
        }

        /// <summary>
        /// 更改门店职员副表信息
        /// </summary>
        /// <param name="staffL">门店职员副表信息</param>
        public void EditEntry(ESSChannelStaff_L staffL)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            session.SaveOrUpdate(staffL);
            session.Flush();

        }

        /// <summary>
        /// 获取门店职员最大号
        /// </summary>
        /// <returns></returns>
        public int StaffMaxId()
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var maxId = session
                .CreateCriteria(typeof(ESSChannelStaff))
                .SetProjection(Projections.Max("FID"))
                .UniqueResult();
            return maxId == null ? 100001 : int.Parse(maxId.ToString()) + 1;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ESSChannelStaff Detail(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var staff = session
                .QueryOver<ESSChannelStaff>()
                .Where(x => x.FID == id)
                .List<ESSChannelStaff>().FirstOrDefault();
            return staff;

        }

        /// <summary>
        /// 删除门店职员
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int Delete(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql2 = "DELETE FROM ESSChannelStaff  WHERE FID =:p1 ";
            string sql1 = "DELETE FROM ESSChannelStaff_L  WHERE FID =:p1 ";
            using (ITransaction transaction = session.BeginTransaction())
            {
                try
                {
                    int result1 = session
                        .CreateQuery(sql2)
                        .SetParameter("p1", id)
                        .ExecuteUpdate();

                    int result = session
                        .CreateQuery(sql1)
                        .SetParameter("p1", id)
                        .ExecuteUpdate();

                    transaction.Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

        }

        public ESSChannelStaff QueryStaffById(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();


            ESSChannelStaff staff = session
                .Query<ESSChannelStaff>()
                .Where(x => x.FID == id)
                .ToList()
                .FirstOrDefault();
            return staff;

        }

        public void UpdateIsNewStatus(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = "UPDATE T_ESS_CHANNELSTAFF SET ISNEW = 1 WHERE FID = :p1";

            session
                 .CreateSQLQuery(sql)
                 .SetParameter("p1", id)
                 .ExecuteUpdate();

        }

        public void UpdateCustomerA3Id(int id, long a3id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = "UPDATE T_ESS_CHANNELSTAFF SET A3ID = :p1 WHERE FID = :p2";

            session
                 .CreateSQLQuery(sql)
                 .SetParameter("p1", a3id)
                 .SetParameter("p2", id)
                 .ExecuteUpdate();

        }

    }
}
