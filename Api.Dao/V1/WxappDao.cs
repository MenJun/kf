using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Model.BO;
using Api.Model.DO;
using Api.Model.VO;
using Api.Model.VO.WX;
using Common.Utils;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace Api.Dao.V1
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019-12-11 09:58:08
	*描  述  ：
	***********************************************************************/
    public class WxappDao
    {
        /// <summary>
        /// 查询分类下的商品
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> QueryGoodsByType(GoodsFilterVO filter)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {0}) RN, A.FID, A.FCODE, A.FNAME, A.FENABLE, A.FCUSABLE, A.FK3CODE, A.FUNIT, A.FPRICE, A.FCONNECT, A.FSPECS, A.FCPTYPE, A.FCPTYPEID, A.FSTOREID, A.FCOMBINATION, A.FAMOUNT, A.FDETAIL, A.FHELPCODE, C.URL IMGPATH 
                                FROM DBO.T_GOODS A
                                LEFT JOIN DBO.T_CLASSIFY B ON B.FID=A.FCPTYPEID
                                LEFT JOIN (SELECT MAX(ID) ID,MAX(URL) URL,COMMODITYID  FROM  DBO.T_GOODSIMAGE GROUP BY COMMODITYID) C ON C.COMMODITYID=A.FID
                                WHERE A.FENABLE=1 AND B.FNAME = :p1 AND A.FNAME LIKE :p2) T WHERE RN >= :p3 AND RN <= :p4";

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                sql = string.Format(sql, "A.FID");
            }
            else
            {
                sql = string.Format(sql, $"{filter.Sort},A.FID");
            }

            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", filter.Type)
                .SetParameter("p2", $"%{filter.Kw}%")
                .SetParameter("p3", (filter.Page - 1) * filter.PageSize + 1)
                .SetParameter("p4", filter.Page * filter.PageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询可定制商品
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> QueryCusableGoods()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT A.*, C.URL IMGPATH FROM dbo.T_Goods A
                                LEFT JOIN(SELECT MAX(Url) URL, CommodityId FROM dbo.T_GoodsImage GROUP BY CommodityId) C ON C.CommodityId = A.fid
                                WHERE A.fenable = 1 AND A.fcusable = 1; ";
            var result = session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询用户待付款、待发货、待收货订单数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IList<dynamic> QueryOrderCountStatusNum(int uid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"DECLARE @DFK INT
DECLARE @DFH INT 
DECLARE @DSH INT 

SELECT @DFK = COUNT(1) FROM (  SELECT  T1.FID,T1.FACTAMOUNT,T1.FNUMBE,T1.FPRODUCER, T1.FORDERDATE, T1.FBUYER, T1.FBUYERID, T1.FDOCUMENTSTATUS,
                    T1.FAMOUNT
FROM      DBO.T_PORDERS AS T1  
WHERE  T1.FPAYMENTWAY = '微信支付' AND  (T1.FKDFS = '普通销售') AND T1.FDOCUMENTSTATUS = 'P' AND T1.FPERSONID = :p1) T  

SELECT @DFH = COUNT(1) FROM (  SELECT  T1.FID,T1.FACTAMOUNT,T1.FNUMBE,T1.FPRODUCER, T1.FORDERDATE, T1.FBUYER, T1.FBUYERID, T1.FDOCUMENTSTATUS, 
                   T1.FAMOUNT,0 PAYAMOUNT
FROM      DBO.T_PORDERS AS T1  
WHERE   (T1.FKDFS = '普通销售') AND T1.FDOCUMENTSTATUS = 'C' AND T1.FPERSONID = :p1 ) T 

SELECT @DSH = COUNT(1) FROM (  SELECT  T1.FID,T1.FACTAMOUNT,T1.FNUMBE,T1.FPRODUCER, T1.FORDERDATE, T1.FBUYER, T1.FBUYERID, T1.FDOCUMENTSTATUS, 
                   T1.FAMOUNT,0 PAYAMOUNT
FROM      DBO.T_PORDERS AS T1  
WHERE   (T1.FKDFS = '普通销售') AND T1.FDOCUMENTSTATUS = 'Q' AND T1.FPERSONID = :p1 ) T 

SELECT @DFK DFK,@DFH DFH, @DSH DSH";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询待支付订单
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="kw">搜索关键字</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<dynamic> QueryWaitingPayOrders(int uid, string kw, int page, int pageSize)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT *
FROM(SELECT *, ROW_NUMBER() OVER (ORDER BY FID DESC) XH
     FROM(SELECT T_PORDERS.FID, T_PORDERS.FNUMBE, T_PORDERS.FORDERDATE,T_PORDERS.FTOTAL FTOTALQTY,T_PORDERS.FACTAMOUNT FTOTALAMOUNT, T_PORDERS.FPERSONID, T_PORDERS.FDOCUMENTSTATUS
          FROM T_PORDERS
               LEFT JOIN(SELECT FORDERSID, FPRODUCTNAME
                         FROM DBO.T_PORDERSENTRY
                         GROUP BY FORDERSID, FPRODUCTNAME) ORDERENTRY ON ORDERENTRY.FORDERSID=DBO.T_PORDERS.FID
          WHERE T_PORDERS.FPAYMENTWAY = '微信支付' AND  (T_PORDERS.FKDFS = '普通销售') AND T_PORDERS.FDOCUMENTSTATUS = 'P' AND (T_PORDERS.FPERSONID=:p1)AND(FNUMBE LIKE :p2 OR ORDERENTRY.FPRODUCTNAME LIKE :p2)
          GROUP BY FORDERDATE, FID, FNUMBE, FPERSONID, FDOCUMENTSTATUS,FTOTAL, FACTAMOUNT) TEMP ) T
WHERE T.XH>:p3 AND T.XH<=:p4;";

            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetParameter("p2", $"%{kw}%")
                .SetParameter("p3", (page - 1) * pageSize)
                .SetParameter("p4", page * pageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询待发货订单
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="kw">搜索关键字</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<dynamic> QueryWaitingSendOrders(int uid, string kw, int page, int pageSize)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT *
FROM(SELECT *, ROW_NUMBER() OVER (ORDER BY FID DESC) XH
     FROM(SELECT T_PORDERS.FID, T_PORDERS.FNUMBE, T_PORDERS.FORDERDATE,T_PORDERS.FTOTAL FTOTALQTY,TH.QTY FRETURNQTY,T_PORDERS.FPAYMENTWAY,T_PORDERS.FACTAMOUNT FTOTALAMOUNT, T_PORDERS.FPERSONID, T_PORDERS.FDOCUMENTSTATUS
          FROM T_PORDERS
               LEFT JOIN(SELECT FORDERSID, FPRODUCTNAME
                         FROM DBO.T_PORDERSENTRY
                         GROUP BY FORDERSID, FPRODUCTNAME) ORDERENTRY ON ORDERENTRY.FORDERSID=DBO.T_PORDERS.FID
			   LEFT JOIN (SELECT FORDERID,SUM(FQTY) QTY  FROM DBO.T_SERVICE WHERE FSTATUS<>'关闭' GROUP BY FORDERID) TH ON TH.FORDERID = DBO.T_PORDERS.FID
          WHERE (T_PORDERS.FKDFS = '普通销售') AND (dbo.T_POrders.FDOCUMENTSTATUS ='C') AND (T_PORDERS.FPERSONID=:p1)AND(FNUMBE LIKE :p2 OR ORDERENTRY.FPRODUCTNAME LIKE :p2)
          GROUP BY FORDERDATE, FID, FNUMBE, FPERSONID, FDOCUMENTSTATUS, FPAYMENTWAY,FTOTAL,TH.QTY, FACTAMOUNT) TEMP ) T
WHERE T.XH>:p3 AND T.XH<=:p4;";

            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetParameter("p2", $"%{kw}%")
                .SetParameter("p3", (page - 1) * pageSize)
                .SetParameter("p4", page * pageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询待签收订单
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="kw"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<dynamic> QueryWaitingSignOrders(int uid, string kw, int page, int pageSize)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT *
FROM(SELECT *, ROW_NUMBER() OVER (ORDER BY FID DESC) XH
     FROM(SELECT T_PORDERS.FID, T_PORDERS.FNUMBE, T_PORDERS.FORDERDATE,T_PORDERS.FTOTAL FTOTALQTY,T_PORDERS.FACTAMOUNT FTOTALAMOUNT, T_PORDERS.FPERSONID, T_PORDERS.FDOCUMENTSTATUS,T_PORDERS.FTRANSPORTORG,T_PORDERS.FTRANSPORTNUM
          FROM T_PORDERS
               LEFT JOIN(SELECT FORDERSID, FPRODUCTNAME
                         FROM DBO.T_PORDERSENTRY
                         GROUP BY FORDERSID, FPRODUCTNAME) ORDERENTRY ON ORDERENTRY.FORDERSID=DBO.T_PORDERS.FID
          WHERE  (T_PORDERS.FKDFS = '普通销售') AND (dbo.T_POrders.FDOCUMENTSTATUS ='Q') AND (T_PORDERS.FPERSONID=:p1)AND(FNUMBE LIKE :p2 OR ORDERENTRY.FPRODUCTNAME LIKE :p2)
          GROUP BY FORDERDATE, FID, FNUMBE, FPERSONID, FDOCUMENTSTATUS,FTOTAL, FACTAMOUNT,FTRANSPORTORG,FTRANSPORTNUM) TEMP ) T
WHERE T.XH>:p3 AND T.XH<=:p4;";

            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetParameter("p2", $"%{kw}%")
                .SetParameter("p3", (page - 1) * pageSize)
                .SetParameter("p4", page * pageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单id</param>
        public int CancelOrder(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = "UPDATE T_PORDERS SET FDOCUMENTSTATUS = 'H' WHERE FID=:p1";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", id)
                .ExecuteUpdate();
            return result;
        }

        /// <summary>
        /// 查询所有订单
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="kw">搜索关键字</param>
        /// <returns></returns>
        public IList<dynamic> QueryAllOrders(int uid, string kw, int page, int pageSize)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT *
FROM(SELECT *, ROW_NUMBER() OVER (ORDER BY FID DESC) XH
     FROM(SELECT T_PORDERS.FID, T_PORDERS.FNUMBE, T_PORDERS.FORDERDATE,T_PORDERS.FTOTAL FTOTALQTY, TH.QTY FRETURNQTY, T_PORDERS.FACTAMOUNT FTOTALAMOUNT, T_PORDERS.FPERSONID, T_PORDERS.FDOCUMENTSTATUS,T_PORDERS.FPAYMENTWAY, T_PORDERS.FTRANSPORTNUM, T_PORDERS.FTRANSPORTORG
          FROM T_PORDERS
               LEFT JOIN(SELECT FORDERSID, FPRODUCTNAME
                         FROM DBO.T_PORDERSENTRY
                         GROUP BY FORDERSID, FPRODUCTNAME) ORDERENTRY ON ORDERENTRY.FORDERSID=DBO.T_PORDERS.FID
			   LEFT JOIN (SELECT FORDERID,SUM(FQTY) QTY  FROM DBO.T_SERVICE WHERE FSTATUS<>'关闭' GROUP BY FORDERID) TH ON TH.FORDERID = DBO.T_PORDERS.FID 
          WHERE(T_PORDERS.FPERSONID=:p1)AND(FNUMBE LIKE :p2 OR ORDERENTRY.FPRODUCTNAME LIKE :p2)
          GROUP BY FORDERDATE, FID, FNUMBE, FPERSONID, FDOCUMENTSTATUS, TH.QTY,FPAYMENTWAY,FTOTAL, FACTAMOUNT,FTRANSPORTNUM,FTRANSPORTORG) TEMP ) T
WHERE T.XH>:p3 AND T.XH<=:p4;";
            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetParameter("p2", $"%{kw}%")
                .SetParameter("p3", (page - 1) * pageSize)
                .SetParameter("p4", page * pageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

   

        /// <summary>
        /// 查询快递公司编码
        /// </summary>
        /// <param name="transportOrg"></param>
        /// <returns></returns>
        public string QueryTransportOrgCode(string transportOrg)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = "SELECT CODE FROM dbo.A_TRANSPORTCOMP WHERE Name LIKE :p1";
            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", $"%{transportOrg}%")
                .List<string>()
                .FirstOrDefault();
            return result;
        }


        /// <summary>
        /// 查询所有订单副表信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IList<dynamic> QueryAllOrdersEntry(int[] ids)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT T_PORDERS.FID, T_PORDERSENTRY.SPID, T_PORDERSENTRY.FID AS ENTRYFID, T_PORDERSENTRY.FCODE, 
T_PORDERSENTRY.FPRODUCTNAME, T_PORDERSENTRY.FSPECIFICATIONS, T_PORDERSENTRY.FLISTPRICE, 
T_PORDERSENTRY.FQTY, ISNULL(RETURNORDER.FQTY, 0) FRETURNQTY, T_PORDERSENTRY.FUNITPRICE, 
T_PORDERSENTRY.FTOTALSUM, T_PORDERSENTRY.FGOODSSTATUS, GI.IMGPATH, G.FCUSABLE, FACTPRICE
FROM T_PORDERS
     INNER JOIN T_PORDERSENTRY ON T_PORDERS.FID=T_PORDERSENTRY.FORDERSID
     LEFT JOIN T_GOODS G ON G.FID=T_PORDERSENTRY.SPID
     LEFT JOIN(SELECT MIN(ID) ID, MIN(URL) IMGPATH, COMMODITYID
               FROM T_GOODSIMAGE
               GROUP BY COMMODITYID) GI ON GI.COMMODITYID=T_PORDERSENTRY.SPID
     LEFT JOIN(SELECT FORDERID, FORDERENTRYID, SUM(FQTY) FQTY
               FROM T_SERVICE
               WHERE FSTATUS<>'已关闭'
               GROUP BY FORDERID, FORDERENTRYID) RETURNORDER ON RETURNORDER.FORDERID=T_PORDERS.FID AND RETURNORDER.FORDERENTRYID=T_PORDERSENTRY.FID
			   WHERE T_PORDERS.FID IN (:p1)";
            var result = session
                .CreateSQLQuery(sql)
                .SetParameterList("p1", ids)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询客户分享信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <returns></returns>
        public IList<dynamic> QueryCustomerShareInfo(string wxopenid, int page, int pageSize)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = "SELECT * FROM (select *,ROW_NUMBER() over(order by id desc) xh from A_SHAREINFO where WXOPENID = :p1) t where  xh > :p2 AND xh <= :p3";
            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", (page - 1) * pageSize)
                .SetParameter("p3", page * pageSize)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询所有已发布的商品
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> QueryAllPublishActivity()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT A.FID, A.FTITLE, A.FPUBLISHDATE, A.FREMARK, B.IMGPATH POSTER, C.IMGPATH SWIPER
FROM dbo.T_SALE_ACTIVITY A
     LEFT JOIN dbo.T_SALE_ACTIVITY_POSTER B ON B.FID=A.FID
     LEFT JOIN(SELECT FID, MAX(IMGPATH) IMGPATH
               FROM dbo.T_SALE_ACTIVITY_SWIPER
               GROUP BY FID) C ON C.FID=A.FID
WHERE A.FPUBLISHSTATUS='1'
ORDER BY FPUBLISHDATE DESC;";
            var result = session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询已发布的指定的商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<dynamic> QueryPublishGoodsById(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT FID, FCODE, FNAME, FENABLE, FK3CODE, FUNIT,FSPECS,FCPTYPEID,FCPTYPE, FPRICE,FCONNECT,FSTOREID,FDETAIL,FUSEABLEPOINT   
                                FROM T_GOODS 
                                WHERE FENABLE='1' AND FID=:p1";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", id)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询商品图片
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public IList<string> QueryGoodsImages(int goodsId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @" SELECT  Url  FROM  T_GoodsImage WHERE CommodityId = :p1 ORDER BY ID DESC";

            var list = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", goodsId)
                .List<string>();
            return list;
        }

 

        /// <summary>
        /// 查询商品活动
        /// </summary>
        /// <param name="actType"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public dynamic QueryGoodsActivity(int goodsId, int actType = 0)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = $"EXEC dbo.sp_hdid  @fs = {actType},@spid = {goodsId}";
            var result = session
                .CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();

            return result;
        }

        /// <summary>
        /// 查询商品sku
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public dynamic QueryGoodsSku(int goodsId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = $"SELECT ID, PRICE,ATTRIBUTE,GOODSID FROM DBO.T_GOODS_ATTRIBUTE WHERE GOODSID = :p1";
            var result = session
                .CreateSQLQuery(sql)
                .SetParameter("p1", goodsId)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return result;
        }

        /// <summary>
        /// 查询用户购物车
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> QueryUserCart(int uid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT DISTINCT A.ID, A.FID, QTY, A.REMARK, A.ISCUSTOM, TYPE, DATE, YS, JD, PL, GG, CZ, ZC, PRICE, RING, GOODSNUMBER, STOCKNUMBER, STOREID,
PERSONID, XHSL, QHSL, CASE ISNULL(A.ZC,'') WHEN 'SKU' THEN A.PRICE ELSE  B.FPRICE END AS LISTPRICE,B.FUSEABLEPOINT, C.URL IMAGE
                              FROM DBO.A_CART A
                              LEFT JOIN DBO.T_GOODS B ON B.FID = A.FID
                              LEFT JOIN (SELECT MAX(URL) URL,COMMODITYID FROM  DBO.T_GOODSIMAGE GROUP BY COMMODITYID) C ON C.COMMODITYID=A.FID WHERE PERSONID=:p1";
            IList<dynamic> datas = session.CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return datas;

        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public dynamic QueryUserInfo(int uid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT KHNAME, FMOBILE, GENDER, BIRTHDAY, FWECHAT, AREA , b.PICTURE, ISNULL(b.USEWXAVATAR,1) USEWXAVATAR FROM DBO.T_ESS_CHANNELSTAFF A 
LEFT JOIN DBO.T_ESS_CHANNELSTAFF_AVATAR B ON B.STAFFID = A.FID WHERE A.FID=:p1";
            var datas = session.CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return datas;
        }

        /// <summary>
        /// 更新用户性别
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public int UpdateUserGender(int uid, string gender)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET GENDER = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", gender)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        public int UpdateUserBirthday(int uid, string birthday)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET BIRTHDAY = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", birthday)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        public int UpdateUserArea(int uid, string area)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET AREA = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", area)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        public int UpdateUserName(int uid, string name)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET KHNAME = :p1 WHERE FID=:p2
                           UPDATE dbo.T_ESS_CHANNELSTAFF_L SET FNAME = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", name)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        public int UpdateUserWechat(int uid, string wechat)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET FWECHAT = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", wechat)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        /// <summary>
        /// 添加分享信息
        /// </summary>
        /// <param name="share"></param>
        public void AddShareInfo(ShareInfo share)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            session.SaveOrUpdate(share);
        }

        /// <summary>
        /// 添加分享浏览信息
        /// </summary>
        /// <param name="share"></param>
        public void AddShareInfoEntry(ShareInfoEntry entry)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            session.SaveOrUpdate(entry);
        }

        /// <summary>
        /// 保存用户头像信息
        /// </summary>
        /// <param name="avatar"></param>
        public void AddOrUpdateStaffAvatar(ESSChannelStaffAvatar avatar)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            session.SaveOrUpdate(avatar);
        }

        public ESSChannelStaff QueryCustomer(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session
                .QueryOver<ESSChannelStaff>()
                .And(x => x.FID == id)
                .List()
                .FirstOrDefault();
        }
        /// <summary>
        /// 查询用户头像信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ESSChannelStaffAvatar QueryStaffAvatar(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session
                .QueryOver<ESSChannelStaffAvatar>()
                .And(x => x.StaffId == id)
                .List<ESSChannelStaffAvatar>()
                .FirstOrDefault();
        }

        /// <summary>
        /// 今日使用积分累计
        /// </summary>
        /// <param name="memberPhone"></param>
        /// <returns></returns>
        public int QueryJrUseRebate(string memberPhone)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT ISNULL(SUM(FPOINT),0) FROM T_PORDERS WHERE FCUSTOMERTEL=:p1 AND (CONVERT(DATE,FORDERDATE,102) = CONVERT(DATE,GETDATE(),102))";
            var list = session
                  .CreateSQLQuery(sql)
                  .SetParameter("p1", memberPhone)
                  .List<int>()
                  .FirstOrDefault();
            return list;
        }

        /// <summary>
        /// 校验订单是否存在
        /// </summary>
        /// <param name="orderNum"></param>
        /// <returns></returns>
        public int CheckOrderExists(string orderNum)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT COUNT(1) FROM dbo.T_POrders WHERE Fnumbe = :p1";
            var list = session
                  .CreateSQLQuery(sql)
                  .SetParameter("p1", orderNum)
                  .List<int>()
                  .FirstOrDefault();
            return list;
        }

   

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderNum"></param>
        public void UpdateOrderStatus(string orderNum)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"DECLARE @totalAmount DECIMAL;
            DECLARE @point INT;
            DECLARE @payAmount DECIMAL;
            SELECT @totalAmount = Factamount, @point = Fpoint FROM dbo.T_POrders WHERE Fnumbe = :p1;
            SELECT @payAmount = ISNULL(SUM(PAYAMOUNT), 0) FROM dbo.T_PORDERS_PAYINFO WHERE ORDERNO = :p1;
            IF(@payAmount + @point >= @totalAmount)
            BEGIN
                UPDATE dbo.T_POrders SET FDocumentStatus = 'B' WHERE Fnumbe = :p1;
            END;";
            session.CreateSQLQuery(sql)
                .SetParameter("p1", orderNum)
                .ExecuteUpdate();
        }

   


        /// <summary>
        /// 查询分享信息
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public dynamic QueryShareGoods(string uuid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT A.ID, A.UUID, A.ACTIVITYID, A.GOODSID, A.WXOPENID, A.DATE,B.KHNAME FROM DBO.A_SHAREINFO A LEFT JOIN DBO.T_ESS_CHANNELSTAFF B ON A.WXOPENID = B.FWXOPENID WHERE a.UUID = :p1";
            return session
                .CreateSQLQuery(sql)
                .SetParameter("p1", uuid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
        }

        /// <summary>
        /// 查询商品图片信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IList<dynamic> QueryGoodsImage(int[] ids)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT FID,FCODE,FNAME,FUNIT,FPRICE,FSPECS,IMGPATH FROM T_Goods a LEFT JOIN (SELECT MIN(ID) ID, MIN(URL) IMGPATH, COMMODITYID
               FROM T_GOODSIMAGE
               GROUP BY COMMODITYID) b on b.CommodityId=a.fid WHERE a.fid in (:p1)";
            return session
                .CreateSQLQuery(sql)
                .SetParameterList("p1", ids)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        /// <summary>
        /// 保存客户消息
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public Task<object> SaveCustomerMessage(CustomerServiceMessage serviceMessage)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session.SaveAsync(serviceMessage);
        }

        public IList<CustomerServiceMessage> QueryCustomerMessage(string wxopenid, int page, int limit)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT a.Id, a.MsgId, a.ToUserName, a.FromUserName, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId
FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.ToUserName, ISNULL(cs.KHNAME, t.FromUserName) FromUserName, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId
     FROM(SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE FromUserName=:p1
          UNION
          SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE ToUserName=:p1) t
         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.FromUserName
         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
WHERE a.xh>:p2 AND xh<=:p3 ORDER BY Id";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", (page - 1) * limit)
                .SetParameter("p3", page * limit)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CustomerServiceMessage)))
                .List<CustomerServiceMessage>();
        }

        public CustomerServiceMessage QueryCustomerLastMessage(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var maxId = DetachedCriteria
                .For<CustomerServiceMessage>()
                .SetProjection(Projections.Max<CustomerServiceMessage>(x => x.Id))
                .Add(Restrictions.Or(Restrictions.Eq("FromUserName", wxopenid), Restrictions.Eq("ToUserName", wxopenid)));

            return session
                .CreateCriteria<CustomerServiceMessage>()
                .Add(Subqueries.PropertyEq("Id", maxId))
                .UniqueResult<CustomerServiceMessage>();
        }

        public IList<CustomerServiceMessage> QueryCustomer()
        {

            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT * FROM (SELECT * FROM dbo.T_CUS_SERVER_MSG WHERE FromUserName=:p1
                           UNION
                           SELECT* FROM dbo.T_CUS_SERVER_MSG WHERE ToUserName = :p1) t ORDER BY t.CreateTime";
            return session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CustomerServiceMessage)))
                .List<CustomerServiceMessage>();
        }
        /// <summary>
        /// 查询客户信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <returns></returns>
        public IList<dynamic> QueryCustomerInfo(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //!返回的字段必须小写
            string sql = @"SELECT a.fid,a.khname,b.picture,a.fwxopenid FROM dbo.T_ESS_CHANNELSTAFF a 
LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR b ON a.FID=b.STAFFID 
WHERE a.FWXOPENID = :p1";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        public IList<dynamic> QueryCustomers()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"select a.*,ISNULL(m1.Content,'[图片]') content,m1.createtime from (SELECT a.FID,a.KHNAME,b.PICTURE,a.FWXOPENID FROM dbo.T_ESS_CHANNELSTAFF a 
LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR b ON a.FID=b.STAFFID 
left join T_ESS_CHANNELSTAFF_L c on c.FID = a.FID
left join A_ROLE d on d.FID = c.FROLEID
WHERE d.FNAME<>'客服' and FWXOPENID IN (SELECT FromUserName FROM dbo.T_CUS_SERVER_MSG WHERE MsgId>0 GROUP BY FromUserName)) a 
LEFT JOIN (select FromUserName, MAX(id) id, MAX(CreateTime) createTime from T_CUS_SERVER_MSG  group by FromUserName) m on m.FromUserName = a.FWXOPENID
LEFT JOIN T_CUS_SERVER_MSG m1 on m1.Id = m.id";
            return session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        public string jjj()
        {
            return "以下新加";
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public dynamic ZXKH_QueryUserInfo(int uid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"SELECT KHNAME, FMOBILE, GENDER, BIRTHDAY, FWECHAT, AREA , b.PICTURE, ISNULL(b.USEWXAVATAR,1) USEWXAVATAR FROM DBO.T_ESS_CHANNELSTAFF A 
LEFT JOIN DBO.T_ESS_CHANNELSTAFF_AVATAR B ON B.STAFFID = A.FID WHERE A.FID=:p1";
            var datas = session.CreateSQLQuery(sql)
                .SetParameter("p1", uid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return datas;
        }


        public int ZXKH_UpdateUserName(int uid, string name)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET KHNAME = :p1 WHERE FID=:p2
                           UPDATE dbo.T_ESS_CHANNELSTAFF_L SET FNAME = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", name)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }

        public int ZXKH_UpdateUserWechat(int uid, string wechat)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"UPDATE dbo.T_ESS_CHANNELSTAFF SET FWECHAT = :p1 WHERE FID=:p2";
            var result = session.CreateSQLQuery(sql)
                .SetParameter("p1", wechat)
                .SetParameter("p2", uid)
                .ExecuteUpdate();
            return result;
        }


        /// <summary>
        /// 保存用户头像信息
        /// </summary>
        /// <param name="avatar"></param>
        public void ZXKH_AddOrUpdateStaffAvatar(ESSChannelStaffAvatar avatar)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            session.SaveOrUpdate(avatar);
        }

        public ESSChannelStaff ZXKH_QueryCustomer(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session
                .QueryOver<ESSChannelStaff>()
                .And(x => x.FID == id)
                .List()
                .FirstOrDefault();
        }

        /// <summary>
        /// 查询用户头像信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ESSChannelStaffAvatar ZXKH_QueryStaffAvatar(int id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session
                .QueryOver<ESSChannelStaffAvatar>()
                .And(x => x.StaffId == id)
                .List<ESSChannelStaffAvatar>()
                .FirstOrDefault();
        }


        /// <summary>
        /// 查询商品图片信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryGoodsImage(int[] ids)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"SELECT FID,FCODE,FNAME,FUNIT,FPRICE,FSPECS,IMGPATH FROM T_Goods a LEFT JOIN (SELECT MIN(ID) ID, MIN(URL) IMGPATH, COMMODITYID
               FROM T_GOODSIMAGE
               GROUP BY COMMODITYID) b on b.CommodityId=a.fid WHERE a.fid in (:p1)";
            return session
                .CreateSQLQuery(sql)
                .SetParameterList("p1", ids)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        /// <summary>
        /// 保存客户消息
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public Task<object> ZXKH_SaveCustomerMessage(CustomerServiceMessage serviceMessage)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            return session.SaveAsync(serviceMessage);
        }

        public IList<CustomerServiceMessage> ZXKH_QueryCustomerMessage(string wxopenid, string openid, int page, int limit)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //    string sql = @"SELECT a.Id, a.MsgId, a.ToUserName, a.FromUserName, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId, a.Format, a.Recognition
            //FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.ToUserName, ISNULL(cs.KHNAME, t.FromUserName) FromUserName, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId, t.Format, t.Recognition
            //FROM(SELECT *
            //  FROM dbo.T_CUS_SERVER_MSG
            //  WHERE FromUserName=:p1 and ToUserName= :p2
            //  UNION
            //  SELECT *
            //  FROM dbo.T_CUS_SERVER_MSG
            //  WHERE ToUserName=:p3 and FromUserName=:p4) t
            // LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.FromUserName
            // LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
            //WHERE a.xh>:p5 AND xh<=:p6 ORDER BY Id";
            string sql = @"SELECT a.Id, a.MsgId, a.XCXToOpenId, a.XCXFromOpenId, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId, a.Format, a.Recognition
        FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.XCXToOpenId, ISNULL(cs.KHNAME, t.XCXFromOpenId) XCXFromOpenId, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId, t.Format, t.Recognition
        FROM(SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXFromOpenId=:p1 and XCXToOpenId= :p2
          UNION
          SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXToOpenId=:p3 and XCXFromOpenId=:p4) t
         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.XCXFromOpenId
         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
        WHERE a.xh>:p5 AND xh<=:p6 ORDER BY Id";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", openid)
                .SetParameter("p3", wxopenid)
                .SetParameter("p4", openid)
                .SetParameter("p5", (page - 1) * limit)
                .SetParameter("p6", page * limit)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CustomerServiceMessage)))
                .List<CustomerServiceMessage>();
        }
        public IList<WxSendMessage> ZXKH_QueryGroupMsg(string wxopenid, int page, int limit)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //            string sql =
            //                @"SELECT a.Id, a.MsgId, a.ToUserName, a.FromUserName, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId,a.PICTURE
            //        FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.ToUserName, ISNULL(cs.KHNAME, t.FromUserName) FromUserName, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId,t.PICTURE
            //     FROM(SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.ToUserName, 
            //                dbo.T_CUS_SERVER_MSG.FromUserName, dbo.T_CUS_SERVER_MSG.CreateTime, 
            //                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
            //                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
            //                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
            //                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE
            //FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
            //                dbo.T_ESS_CHANNELSTAFF ON 
            //                dbo.T_CUS_SERVER_MSG.FromUserName = dbo.T_ESS_CHANNELSTAFF.FWXOPENID INNER JOIN
            //                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
            //                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
            //WHERE   (dbo.T_CUS_SERVER_MSG.FromUserName = :p1)
            //          UNION
            //          SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.ToUserName, 
            //                dbo.T_CUS_SERVER_MSG.FromUserName, dbo.T_CUS_SERVER_MSG.CreateTime, 
            //                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
            //                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
            //                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
            //                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE
            //FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
            //                dbo.T_ESS_CHANNELSTAFF ON 
            //                dbo.T_CUS_SERVER_MSG.FromUserName = dbo.T_ESS_CHANNELSTAFF.FWXOPENID INNER JOIN
            //                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
            //                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
            //WHERE   (dbo.T_CUS_SERVER_MSG.ToUserName = :p1)) t
            //         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.FromUserName
            //         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
            //WHERE a.xh>:p2 AND xh<=:p3 ORDER BY Id";
            string sql = @"SELECT a.Id, a.MsgId, a.XCXToOpenId, a.XCXFromOpenId, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId,a.PICTURE
        FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.XCXToOpenId, ISNULL(cs.KHNAME, t.XCXFromOpenId) XCXFromOpenId, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId,t.PICTURE
     FROM(SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.CreateTime, 
                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE
FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
WHERE   (dbo.T_CUS_SERVER_MSG.XCXFromOpenId = :p1)
          UNION
          SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.CreateTime, 
                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE
FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
WHERE   (dbo.T_CUS_SERVER_MSG.XCXToOpenId = :p1)) t
         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.XCXFromOpenId
         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
WHERE a.xh>:p2 AND xh<=:p3 ORDER BY Id";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", (page - 1) * limit)
                .SetParameter("p3", page * limit)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(WxSendMessage)))
                .List<WxSendMessage>();
        }


        public CustomerServiceMessage ZXKH_QueryCustomerLastMessage(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            var maxId = DetachedCriteria
                .For<CustomerServiceMessage>()
                .SetProjection(Projections.Max<CustomerServiceMessage>(x => x.Id))
                .Add(Restrictions.Or(Restrictions.Eq("FromUserName", wxopenid),
                    Restrictions.Eq("ToUserName", wxopenid)));

            return session
                .CreateCriteria<CustomerServiceMessage>()
                .Add(Subqueries.PropertyEq("Id", maxId))
                .UniqueResult<CustomerServiceMessage>();
        }

        public IList<CustomerServiceMessage> ZXKH_QueryCustomer()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT * FROM (SELECT * FROM dbo.T_CUS_SERVER_MSG WHERE FromUserName=:p1
                           UNION
                           SELECT* FROM dbo.T_CUS_SERVER_MSG WHERE ToUserName = :p1) t ORDER BY t.CreateTime";
            return session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CustomerServiceMessage)))
                .List<CustomerServiceMessage>();
        }

        /// <summary>
        /// 查询客户信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryCustomerInfo(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //!返回的字段必须小写
            string sql = @"SELECT a.fid,a.khname,b.picture,a.fwxopenid FROM dbo.T_ESS_CHANNELSTAFF a 
LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR b ON a.FID=b.STAFFID 
WHERE a.FWXOPENID = :p1";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }
        /// <summary>
        /// 所有客户发送给用户的消息，非群聊
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryCustomers()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //string sql = @"SELECT     TOP (100) PERCENT a_1.FID, a_1.KHNAME, a_1.PICTURE, a_1.XCXOPENID, a_1.FWXOPENID, ISNULL(m1.[Content], '[图片]') AS [content], m1.CreateTime
            //    FROM         (SELECT     a.FID, a.KHNAME, b.PICTURE, a.XCXOPENID, a.FWXOPENID
            //           FROM          dbo.T_ESS_CHANNELSTAFF AS a LEFT OUTER JOIN
            //                                  dbo.T_ESS_CHANNELSTAFF_AVATAR AS b ON a.FID = b.STAFFID LEFT OUTER JOIN
            //                                  dbo.T_ESS_CHANNELSTAFF_L AS c ON c.FID = a.FID LEFT OUTER JOIN
            //                                  dbo.A_ROLE AS d ON d.FID = c.FROLEID
            //           WHERE      (a.XCXOPENID IN
            //                                      (SELECT     XCXFromOpenId
            //                                        FROM          dbo.T_CUS_SERVER_MSG
            //                                        WHERE      (MsgId >= 0) AND (LEN(XCXToOpenId) = 28)
            //                                        GROUP BY XCXFromOpenId))) AS a_1 LEFT OUTER JOIN
            //              (SELECT     XCXFromOpenId, MAX(Id) AS id, MAX(CreateTime) AS CreateTime
            //                FROM          dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
            //                WHERE      (LEN(XCXToOpenId) = 28)
            //                GROUP BY XCXFromOpenId) AS m ON m.XCXFromOpenId = a_1.XCXOPENID LEFT OUTER JOIN
            //          dbo.T_CUS_SERVER_MSG AS m1 ON m1.Id = m.id
            //        ORDER BY m1.CreateTime DESC";
            string sql = @"SELECT     TOP (100) PERCENT a_1.FID, a_1.KHNAME, a_1.PICTURE, a_1.XCXOPENID, a_1.FWXOPENID, ISNULL(m1.[Content], '[图片]') AS [Content], m1.CreateTime
                FROM         (SELECT     a.FID, a.KHNAME, b.PICTURE, a.XCXOPENID, a.FWXOPENID
                       FROM          dbo.T_ESS_CHANNELSTAFF AS a LEFT OUTER JOIN
                                              dbo.T_ESS_CHANNELSTAFF_AVATAR AS b ON a.FID = b.STAFFID LEFT OUTER JOIN
                                              dbo.T_ESS_CHANNELSTAFF_L AS c ON c.FID = a.FID LEFT OUTER JOIN
                                              dbo.A_ROLE AS d ON d.FID = c.FROLEID
                       WHERE      (a.XCXOPENID IN
                                                  (SELECT     XCXFromOpenId
                                                    FROM          dbo.T_CUS_SERVER_MSG
                                                    WHERE      (MsgId >= 0) AND (LEN(XCXToOpenId) = 28)
                                                    GROUP BY XCXFromOpenId))) AS a_1 LEFT OUTER JOIN
                          (SELECT     XCXFromOpenId, MAX(Id) AS id, MAX(CreateTime) AS CreateTime
                            FROM          dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
                            WHERE      (LEN(XCXToOpenId) = 28)
                            GROUP BY XCXFromOpenId) AS m ON m.XCXFromOpenId = a_1.XCXOPENID LEFT OUTER JOIN
                      dbo.T_CUS_SERVER_MSG AS m1 ON m1.Id = m.id
                        union
                SELECT   derivedtbl_1.UserFID AS FID, derivedtbl_1.GroupName AS KHNAME, derivedtbl_1.GroupImgBase64 AS PICTURE, 
                derivedtbl_1.GroupNo AS XCXOPENID, derivedtbl_1.GroupNo AS FWXOPENID, derivedtbl_2.[Content], 
                derivedtbl_2.CreateTime
                FROM      (SELECT   TOP (100) PERCENT GroupID, GroupName, GroupQRcode, GroupRemarks, GroupNo, UserFID, createtime, 
                                 GroupImgBase64, GroupState
                 FROM      dbo.T_ESS_CHANNELSTAFF_GROUP
                 WHERE   (GroupState = N'正常')
                 ORDER BY createtime DESC) AS derivedtbl_1 INNER JOIN
                    (SELECT   dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.CreateTime, 
                                     dbo.T_CUS_SERVER_MSG.XCXToOpenId
                     FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                                         (SELECT   XCXToOpenId, MAX(Id) AS Id
                                          FROM      dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
                                          GROUP BY XCXToOpenId
                                          HAVING   ({ fn LENGTH(XCXToOpenId) } > '28')) AS grouplist ON 
                                     dbo.T_CUS_SERVER_MSG.Id = grouplist.Id) AS derivedtbl_2 ON 
                derivedtbl_1.GroupNo = derivedtbl_2.XCXToOpenId
                ORDER BY m1.CreateTime DESC";
            return session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }
        /// <summary>
        /// 我的客户消息
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryMyCustomers(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //string sql =
            //    @"SELECT   a_1.FID, a_1.KHNAME, a_1.PICTURE, a_1.FWXOPENID, ISNULL(m1.Content, '[图片]') AS content, m1.CreateTime
            //      FROM      (SELECT   a.FID, a.KHNAME, b.PICTURE, a.FWXOPENID
            //      FROM      T_ESS_CHANNELSTAFF a LEFT OUTER JOIN
            //                     T_ESS_CHANNELSTAFF_AVATAR b ON a.FID = b.STAFFID LEFT OUTER JOIN
            //                     T_ESS_CHANNELSTAFF_L c ON c.FID = a.FID LEFT OUTER JOIN
            //                     A_ROLE d ON d.FID = c.FROLEID
            //     WHERE   (a.FWXOPENID IN
            //                         (SELECT   FromUserName
            //                          FROM      T_CUS_SERVER_MSG
            //                          WHERE   (MsgId >= 0)
            //                          GROUP BY FromUserName))) a_1 INNER JOIN
            //        (SELECT   T_ESS_CHANNELSTAFF_RelationShip.StaffID, T_ESS_CHANNELSTAFF_RelationShip.CustomerID
            //         FROM      T_ESS_CHANNELSTAFF_RelationShip INNER JOIN
            //                             (SELECT   T_ESS_CHANNELSTAFF.FID, T_ESS_CHANNELSTAFF.FMOBILE
            //                              FROM      T_ESS_CHANNELSTAFF_L INNER JOIN
            //                                              T_ESS_CHANNELSTAFF ON 
            //                                              T_ESS_CHANNELSTAFF_L.FID = T_ESS_CHANNELSTAFF.FID INNER JOIN
            //                                              T_ESS_CHANNELSTAFF_AVATAR ON 
            //                                              T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_AVATAR.STAFFID INNER JOIN
            //                                              A_ROLE ON T_ESS_CHANNELSTAFF_L.FROLEID = A_ROLE.FID
            //                              WHERE   (T_ESS_CHANNELSTAFF.FMOBILE = :p1 )) staff_1 ON 
            //                         T_ESS_CHANNELSTAFF_RelationShip.StaffID = staff_1.FID) customer ON a_1.FID = customer.CustomerID LEFT OUTER JOIN
            //        (SELECT   FromUserName, MAX(Id) AS id, MAX(CreateTime) AS createTime
            //         FROM      T_CUS_SERVER_MSG T_CUS_SERVER_MSG_1
            //         GROUP BY FromUserName) m ON m.FromUserName = a_1.FWXOPENID LEFT OUTER JOIN
            //     T_CUS_SERVER_MSG m1 ON m1.Id = m.id";
            string sql = @"select * from (
                SELECT  ROW_NUMBER()   
                over   
                (PARTITION By T_ESS_CHANNELSTAFF_1.FID order by dbo.T_CUS_SERVER_MSG.CreateTime DESC) as rowId, T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, 
                dbo.T_CUS_SERVER_MSG.[Content] AS [content], dbo.T_CUS_SERVER_MSG.CreateTime
                FROM      dbo.T_ESS_CHANNELSTAFF_AVATAR INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID = T_ESS_CHANNELSTAFF_1.FID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID ON 
                T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID LEFT OUTER JOIN
                dbo.T_CUS_SERVER_MSG ON 
                T_ESS_CHANNELSTAFF_1.XCXOPENID = dbo.T_CUS_SERVER_MSG.XCXFromOpenId AND 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID = dbo.T_CUS_SERVER_MSG.XCXToOpenId
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)
                GROUP BY T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, dbo.T_CUS_SERVER_MSG.[Content], 
                dbo.T_CUS_SERVER_MSG.CreateTime
                ) t
                where rowid <= 1";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }

        /// <summary>
        /// 我的客服消息
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryMyStaff(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"select * from (
                SELECT  ROW_NUMBER()   
                over   
                (PARTITION By T_ESS_CHANNELSTAFF_1.FID order by dbo.T_CUS_SERVER_MSG.CreateTime DESC) as rowId,T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, 
                dbo.T_CUS_SERVER_MSG.[Content] AS [content], dbo.T_CUS_SERVER_MSG.CreateTime
                FROM      dbo.T_CUS_SERVER_MSG RIGHT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID = T_ESS_CHANNELSTAFF_1.FID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID ON 
                T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID ON 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId = T_ESS_CHANNELSTAFF_1.XCXOPENID AND 
                dbo.T_CUS_SERVER_MSG.XCXToOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)
                GROUP BY T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, dbo.T_CUS_SERVER_MSG.[Content], 
                dbo.T_CUS_SERVER_MSG.CreateTime
                ) t
                where rowid <= 1";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }

        /// <summary>
        /// 我的所有消息,用手机号查询出所有用户发送给自己的消息  --目前修改为好友消息及其群消息
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryMyMessage(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //string sql = @"SELECT     TOP (100) PERCENT T_ESS_CHANNELSTAFF_2.FID, T_ESS_CHANNELSTAFF_2.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, T_ESS_CHANNELSTAFF_2.XCXOPENID, 
            //          T_ESS_CHANNELSTAFF_2.FWXOPENID, T_CUS_SERVER_MSG_2.[Content], T_CUS_SERVER_MSG_2.CreateTime
            //            FROM         (SELECT     derivedtbl_1.XCXFromOpenId, derivedtbl_1.XCXToOpenId, MAX(T_CUS_SERVER_MSG_1.Id) AS id
            //           FROM          (SELECT     dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, dbo.T_ESS_CHANNELSTAFF.FMOBILE
            //                                   FROM          dbo.T_CUS_SERVER_MSG INNER JOIN
            //                                                          dbo.T_ESS_CHANNELSTAFF ON dbo.T_CUS_SERVER_MSG.XCXToOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID
            //                                   GROUP BY dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, dbo.T_ESS_CHANNELSTAFF.FMOBILE
            //                                   HAVING      (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)) AS derivedtbl_1 INNER JOIN
            //                                  dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1 ON derivedtbl_1.XCXFromOpenId = T_CUS_SERVER_MSG_1.XCXFromOpenId AND 
            //                                  derivedtbl_1.XCXToOpenId = T_CUS_SERVER_MSG_1.XCXToOpenId CROSS JOIN
            //                                  dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1
            //           GROUP BY derivedtbl_1.XCXFromOpenId, derivedtbl_1.XCXToOpenId) AS lastmess INNER JOIN
            //          dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_2 ON lastmess.id = T_CUS_SERVER_MSG_2.Id INNER JOIN
            //          dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_2 ON lastmess.XCXFromOpenId = T_ESS_CHANNELSTAFF_2.XCXOPENID INNER JOIN
            //          dbo.T_ESS_CHANNELSTAFF_AVATAR ON T_ESS_CHANNELSTAFF_2.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
            //            ORDER BY T_CUS_SERVER_MSG_2.CreateTime DESC";
            string sql = @"SELECT     TOP (100) PERCENT T_ESS_CHANNELSTAFF_2.FID, T_ESS_CHANNELSTAFF_2.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, T_ESS_CHANNELSTAFF_2.XCXOPENID, 
                      T_ESS_CHANNELSTAFF_2.FWXOPENID, T_CUS_SERVER_MSG_2.[Content], T_CUS_SERVER_MSG_2.CreateTime
                        FROM         (SELECT     derivedtbl_1.XCXFromOpenId, derivedtbl_1.XCXToOpenId, MAX(T_CUS_SERVER_MSG_1.Id) AS id
                       FROM          (SELECT     dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, dbo.T_ESS_CHANNELSTAFF.FMOBILE
                                               FROM          dbo.T_CUS_SERVER_MSG INNER JOIN
                                                                      dbo.T_ESS_CHANNELSTAFF ON dbo.T_CUS_SERVER_MSG.XCXToOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID
                                               GROUP BY dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, dbo.T_ESS_CHANNELSTAFF.FMOBILE
                                               HAVING      (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)) AS derivedtbl_1 INNER JOIN
                                              dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1 ON derivedtbl_1.XCXFromOpenId = T_CUS_SERVER_MSG_1.XCXFromOpenId AND 
                                              derivedtbl_1.XCXToOpenId = T_CUS_SERVER_MSG_1.XCXToOpenId CROSS JOIN
                                              dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1
                       GROUP BY derivedtbl_1.XCXFromOpenId, derivedtbl_1.XCXToOpenId) AS lastmess INNER JOIN
                      dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_2 ON lastmess.id = T_CUS_SERVER_MSG_2.Id INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_2 ON lastmess.XCXFromOpenId = T_ESS_CHANNELSTAFF_2.XCXOPENID INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF_AVATAR ON T_ESS_CHANNELSTAFF_2.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                       union
                SELECT   derivedtbl_1.FID, derivedtbl_1.GroupName AS KHNAME, derivedtbl_1.GroupImgBase64 AS PICTURE, 
                derivedtbl_1.GroupNo AS XCXOPENID, derivedtbl_1.GroupNo AS FWXOPENID, derivedtbl_2.[Content], 
                derivedtbl_2.CreateTime
                FROM      (SELECT   TOP (100) PERCENT dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, 
                                 dbo.T_ESS_CHANNELSTAFF.XCXOPENID, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo, 
                                 dbo.T_ESS_CHANNELSTAFF_GROUP.GroupName, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupImgBase64, 
                                 dbo.T_ESS_CHANNELSTAFF_GROUP.createtime, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState
                 FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                                 dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
                                 dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID INNER JOIN
                                 dbo.T_ESS_CHANNELSTAFF_GROUP ON 
                                 dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo
                 WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1) AND 
                                 (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState = N'正常')) AS derivedtbl_1 INNER JOIN
                    (SELECT   dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.CreateTime, 
                                     dbo.T_CUS_SERVER_MSG.XCXToOpenId
                     FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                                         (SELECT   XCXToOpenId, MAX(Id) AS Id
                                          FROM      dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
                                          GROUP BY XCXToOpenId
                                          HAVING   ({ fn LENGTH(XCXToOpenId) } > '28')) AS grouplist ON 
                                     dbo.T_CUS_SERVER_MSG.Id = grouplist.Id) AS derivedtbl_2 ON 
                derivedtbl_1.GroupNo = derivedtbl_2.XCXToOpenId
            ORDER BY T_CUS_SERVER_MSG_2.CreateTime DESC";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 查找出自己所有的客户fid
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryMyCustomerFid(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   dbo.T_ESS_CHANNELSTAFF.FMOBILE, dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 查找出自己所有的客服fid
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryMyStaffFid(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   dbo.T_ESS_CHANNELSTAFF.FMOBILE, dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }


        /// <summary>
        /// 好友列表
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_Friends(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID,T_ESS_CHANNELSTAFF_1.FWXOPENID, dbo.T_ESS_CHANNELSTAFF_FriendShip.status
                FROM      dbo.T_ESS_CHANNELSTAFF_FriendShip INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_ESS_CHANNELSTAFF_FriendShip.userID = dbo.T_ESS_CHANNELSTAFF.FID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON 
                dbo.T_ESS_CHANNELSTAFF_FriendShip.hisfriendID = T_ESS_CHANNELSTAFF_1.FID LEFT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1) AND (dbo.T_ESS_CHANNELSTAFF_FriendShip.status = 1)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }

        public IList<dynamic> ZXKH_User()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT     dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, dbo.T_ESS_CHANNELSTAFF.XCXOPENID, 
                      dbo.T_ESS_CHANNELSTAFF.FWXOPENID
                      FROM         dbo.T_ESS_CHANNELSTAFF INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF_AVATAR ON dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                       WHERE     (dbo.T_ESS_CHANNELSTAFF.XCXOPENID != '-1')";
            return session.CreateSQLQuery(sql)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 查看好友申请
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryApply(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, dbo.T_ESS_CHANNELSTAFF_FriendShip.status
                FROM      dbo.T_ESS_CHANNELSTAFF_AVATAR RIGHT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_FriendShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_FriendShip.hisfriendID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON 
                dbo.T_ESS_CHANNELSTAFF_FriendShip.userID = T_ESS_CHANNELSTAFF_1.FID ON 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID = T_ESS_CHANNELSTAFF_1.FID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1) AND (dbo.T_ESS_CHANNELSTAFF_FriendShip.status = 0)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }


        /// <summary>
        /// 群组列表
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_GroupList(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //string sql = @"SELECT   TOP (100) PERCENT dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, 
            //    dbo.T_ESS_CHANNELSTAFF.FWXOPENID, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo, 
            //    dbo.T_ESS_CHANNELSTAFF_GROUP.GroupName, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupImgBase64, 
            //    dbo.T_ESS_CHANNELSTAFF_GROUP.createtime, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState
            //    FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
            //    dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
            //    dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID INNER JOIN
            //    dbo.T_ESS_CHANNELSTAFF_GROUP ON 
            //    dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo
            //    WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1) AND 
            //    (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState = N'正常')
            //    ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUP.createtime DESC";

            string sql = @"SELECT   TOP (100) PERCENT dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo, 
                dbo.T_ESS_CHANNELSTAFF_GROUP.GroupName, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupImgBase64, 
                dbo.T_ESS_CHANNELSTAFF_GROUP.createtime, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUP ON 
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FMOBILE = :p1) AND 
                (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState = N'正常')
                ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUP.createtime DESC";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 群组列表所有正常状态的
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_ALLGroupList()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   TOP (100) PERCENT GroupID, GroupName, GroupQRcode, GroupRemarks, GroupNo, UserFID, createtime, 
                            GroupImgBase64, GroupState
                            FROM      dbo.T_ESS_CHANNELSTAFF_GROUP
                            WHERE   (GroupState = N'正常')
                            ORDER BY createtime DESC";
            return session.CreateSQLQuery(sql)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }

        /// <summary>
        /// 群头像base保存
        /// </summary>
        /// <returns></returns>
        public void ZXKH_GroupImgBase64(string groupno, string Imgbase64)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"update T_ESS_CHANNELSTAFF_GROUP set  GroupImgBase64=:p1 where GroupNo=:p2";
            session.CreateSQLQuery(sql)
           .SetParameter("p1", Imgbase64)
           .SetParameter("p2", groupno)
           .ExecuteUpdate();
        }

        /// <summary>
        /// 群组图片处理 群成员最先进入九人
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_GroupImg(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   TOP (9) dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE
                FROM      dbo.T_ESS_CHANNELSTAFF_GROUPSHIP INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUP ON 
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                WHERE   (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = :p1)
                ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.JoinTime";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", groupno)
           .List<dynamic>();
        }
        /// <summary>
        /// 群组名称处理
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_GroupName(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   TOP (100) PERCENT dbo.T_ESS_CHANNELSTAFF.KHNAME
                FROM      dbo.T_ESS_CHANNELSTAFF_GROUP INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
                dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID = dbo.T_ESS_CHANNELSTAFF.FID
                WHERE   (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = :p1)
                ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.JoinTime";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", groupno)
           .List<dynamic>();
        }
        /// <summary>
        /// 群组消息显示处理
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> ZXKH_GroupBy(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   top(1) Content, CreateTime
                           FROM      T_CUS_SERVER_MSG
                           WHERE   (XCXToOpenId = :p1)
                           ORDER BY CreateTime DESC";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", groupno)
           .List<dynamic>();
        }

        /// <summary>
        /// 通过手机号码查询此人
        /// </summary>
        /// <param name="FMOBILE"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryUser(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   T_ESS_CHANNELSTAFF.FID, T_ESS_CHANNELSTAFF_AVATAR.PICTURE, T_ESS_CHANNELSTAFF.KHNAME, 
                T_ESS_CHANNELSTAFF.FMOBILE
FROM      T_ESS_CHANNELSTAFF INNER JOIN
                T_ESS_CHANNELSTAFF_AVATAR ON T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_AVATAR.STAFFID
WHERE   (T_ESS_CHANNELSTAFF.FMOBILE = :p1)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", fmobile)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 查询是否为好友关系
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userfriend"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryFriend(long user, long userfriend)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT     friendsID, userID, hisfriendID, createtime, status
                            FROM         dbo.T_ESS_CHANNELSTAFF_FriendShip
                            WHERE     userID = :p1 AND hisfriendID = :p2";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", user)
            .SetParameter("p2", userfriend)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 查询好友申请
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userfriend"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_QueryFriendApply(long user, long userfriend)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"select * from T_ESS_CHANNELSTAFF_FriendShip where userID = :p1 and hisfriendID = :p2";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", user)
            .SetParameter("p2", userfriend)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }
        /// <summary>
        /// 发送好友申请
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userfriend"></param>
        /// <returns></returns>
        public void ZXKH_SendApply(long user, long userfriend)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"insert into T_ESS_CHANNELSTAFF_FriendShip(userID,hisfriendID,createtime,status) values(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()),0)";
            session.CreateSQLQuery(sql)
            .SetParameter("p1", user)
            .SetParameter("p2", userfriend)
            .ExecuteUpdate();
        }
        /// <summary>
        /// 添加好友关系 相互关系
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>
        public void ZXKH_Friendadd(long user, long userfriend)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"update T_ESS_CHANNELSTAFF_FriendShip set status = 1 where userID = :p1 and hisfriendID = :p2";
            session.CreateSQLQuery(sql)
            .SetParameter("p1", userfriend)
            .SetParameter("p2", user)
            .ExecuteUpdate();


            string sql1 = @"insert into T_ESS_CHANNELSTAFF_FriendShip(userID,hisfriendID,createtime,status) values(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()),1)";
            session.CreateSQLQuery(sql1)
            .SetParameter("p1", user)
            .SetParameter("p2", userfriend)
            .ExecuteUpdate();

            string sql2 = @"delete from T_ESS_CHANNELSTAFF_FriendShip where userID = :p1 and hisfriendID = :p2 and status = 0";
            session.CreateSQLQuery(sql2)
            .SetParameter("p1", user)
            .SetParameter("p2", userfriend)
            .ExecuteUpdate();
        }

        /// <summary>
        /// 创建群组
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>
        public void ZXKH_GroupCreat(long array, string groupNo)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"insert into T_ESS_CHANNELSTAFF_GROUP(GroupNo,UserFID,createtime,GroupState) values(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()),'正常')";
            session.CreateSQLQuery(sql)
           .SetParameter("p1", groupNo)
           .SetParameter("p2", array)
           .ExecuteUpdate();
        }
        /// <summary>
        /// 群组与用户关联
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>
        public void ZXKH_GroupandUser(long array, string groupNo)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"insert into T_ESS_CHANNELSTAFF_GROUPSHIP(GroupNo,UserFID,JoinTime) values(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()))";
            session.CreateSQLQuery(sql)
           .SetParameter("p1", groupNo)
           .SetParameter("p2", array)
           .ExecuteUpdate();
        }


        public void ZXKH_KHDel(string fromUserName)
        {
            ISession session = NHSessionProvider.SessionFactory.OpenSession();
            var sql1 = @"SELECT   FID
                         FROM dbo.T_ESS_CHANNELSTAFF
                         WHERE   (FWXOPENID =  '" + fromUserName + "')";
            long id = session.CreateSQLQuery(sql1).List<Int64>().FirstOrDefault();//执行查询
            var sql2 = @"DELETE  FROM T_ESS_CHANNELSTAFF  WHERE   ( FWXOPENID= '" + fromUserName + "')";
            session.CreateSQLQuery(sql2).ExecuteUpdate();//执行删除
            var sql3 = @"DELETE  FROM T_ESS_CHANNELSTAFF_AVATAR  WHERE   (STAFFID =" + id + ")";
            session.CreateSQLQuery(sql3).ExecuteUpdate();//执行删除
            var sql4 = @"DELETE  FROM T_ESS_CHANNELSTAFF_L  WHERE   (FID =" + id + ")";
            session.CreateSQLQuery(sql4).ExecuteUpdate();//执行删除
        }

        public IList<dynamic> ZXKH_QueryKhCustomer(string fmobile)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"SELECT   a_1.FID, a_1.KHNAME, a_1.PICTURE, a_1.FWXOPENID, ISNULL(m1.Content, '[图片]') AS content, m1.CreateTime
FROM      (SELECT   a.FID, a.KHNAME, b.PICTURE, a.FWXOPENID
                 FROM      T_ESS_CHANNELSTAFF a LEFT OUTER JOIN
                                 T_ESS_CHANNELSTAFF_AVATAR b ON a.FID = b.STAFFID LEFT OUTER JOIN
                                 T_ESS_CHANNELSTAFF_L c ON c.FID = a.FID LEFT OUTER JOIN
                                 A_ROLE d ON d.FID = c.FROLEID
                 WHERE   (a.FWXOPENID IN
                                     (SELECT   FromUserName
                                      FROM      T_CUS_SERVER_MSG
                                      WHERE   (MsgId > 0)
                                      GROUP BY FromUserName))) a_1 INNER JOIN
                    (SELECT   T_ESS_CHANNELSTAFF_RelationShip.StaffID, T_ESS_CHANNELSTAFF_RelationShip.CustomerID
                     FROM      T_ESS_CHANNELSTAFF_RelationShip INNER JOIN
                                         (SELECT   T_ESS_CHANNELSTAFF.FID, T_ESS_CHANNELSTAFF.FMOBILE
                                          FROM      T_ESS_CHANNELSTAFF_L INNER JOIN
                                                          T_ESS_CHANNELSTAFF ON 
                                                          T_ESS_CHANNELSTAFF_L.FID = T_ESS_CHANNELSTAFF.FID INNER JOIN
                                                          T_ESS_CHANNELSTAFF_AVATAR ON 
                                                          T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_AVATAR.STAFFID INNER JOIN
                                                          A_ROLE ON T_ESS_CHANNELSTAFF_L.FROLEID = A_ROLE.FID
                                          WHERE   (T_ESS_CHANNELSTAFF.FMOBILE = " + fmobile + @")) staff_1 ON 
                                     T_ESS_CHANNELSTAFF_RelationShip.CustomerID = staff_1.FID) customer ON a_1.FID = customer.CustomerID LEFT OUTER JOIN
                    (SELECT   FromUserName, MAX(Id) AS id, MAX(CreateTime) AS createTime
                     FROM      T_CUS_SERVER_MSG T_CUS_SERVER_MSG_1
                     GROUP BY FromUserName) m ON m.FromUserName = a_1.FWXOPENID LEFT OUTER JOIN
                T_CUS_SERVER_MSG m1 ON m1.Id = m.id";
            return session.CreateSQLQuery(sql)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }


        public static void ZXKH_savemessage(CustomerServiceMessage WxXmlModels)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            session.Save(WxXmlModels);
            session.Flush();
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
                //@"SELECT   T_ESS_CHANNELSTAFF.FID, T_ESS_CHANNELSTAFF.KHNAME, T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                //T_ESS_CHANNELSTAFF.FWXOPENID
                //FROM      T_ESS_CHANNELSTAFF_AVATAR INNER JOIN
                //T_ESS_CHANNELSTAFF ON T_ESS_CHANNELSTAFF_AVATAR.STAFFID = T_ESS_CHANNELSTAFF.FID INNER JOIN
                //T_ESS_CHANNELSTAFF_L ON T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_L.FID INNER JOIN
                //A_ROLE ON T_ESS_CHANNELSTAFF_L.FROLEID = A_ROLE.FID INNER JOIN
                //T_ESS_CHANNELSTAFF_RelationShip ON T_ESS_CHANNELSTAFF.FID = T_ESS_CHANNELSTAFF_RelationShip.CustomerID
                //WHERE   T_ESS_CHANNELSTAFF.FWXOPENID  = :p1";
                @"select ShipID,StaffID,CustomerID from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = (select fid from T_ESS_CHANNELSTAFF where FWXOPENID=:p1)";
            return session.CreateSQLQuery(sql)
               .SetParameter("p1", FWXOPENID)
               .SetResultTransformer(new AliasToEntityMapResultTransformer())
               .List<dynamic>();
        }
        public void ZXKH_Delete_Customers(string FWXOPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql =
                @"delete from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = (select fid from T_ESS_CHANNELSTAFF where FWXOPENID=:p1)";
            session.CreateSQLQuery(sql)
            .SetParameter("p1", FWXOPENID)
            .ExecuteUpdate();
        }
        /// <summary>
        /// 添加用户和管理者的关联
        /// </summary>
        /// <param name="FWXOPENID">用户id</param>
        public void ZXKH_AddShip(string FWXOPENID, int FPKID, long timestamp)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"insert into T_ESS_CHANNELSTAFF_RelationShip values(:p2,(select FID from T_ESS_CHANNELSTAFF WHERE FWXOPENID = :p1),:p3)";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", FWXOPENID)
                   .SetParameter("p2", FPKID)
                   .SetParameter("p3", timestamp)
                   .ExecuteUpdate();
        }
        /// <summary>
        /// 修改用户和管理者的关联
        /// </summary>
        /// <param name="FWXOPENID">用户id</param>
        public void ZXKH_EditShip(string OPENID, int FPKID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"update T_ESS_CHANNELSTAFF_RelationShip set StaffID=:p2 where CusTomerId = (select FID from T_ESS_CHANNELSTAFF where FWXOPENID =:p1)";
            session.CreateSQLQuery(sql)
                   .SetParameter("p2", FPKID)
                   .SetParameter("p1", OPENID)
                   .ExecuteUpdate();
        }
        /// <summary>
        /// 查找出所有的客服按聊天时间倒叙
        /// </summary>
        /// <param name="FWXOPENID"></param>
        public IList<dynamic> ZXKH_SelectStaff()
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   a_1.FID, m1.CreateTime
                           FROM      (SELECT   a.FID, a.KHNAME, b.PICTURE, a.FWXOPENID
                           FROM      dbo.T_ESS_CHANNELSTAFF AS a LEFT OUTER JOIN
                                 dbo.T_ESS_CHANNELSTAFF_AVATAR AS b ON a.FID = b.STAFFID LEFT OUTER JOIN
                                 dbo.T_ESS_CHANNELSTAFF_L AS c ON c.FID = a.FID LEFT OUTER JOIN
                                 dbo.A_ROLE AS d ON d.FID = c.FROLEID
                           WHERE   (d.FPERMISSIONS = 1) AND (a.FWXOPENID IN
                                     (SELECT   FromUserName
                                      FROM      dbo.T_CUS_SERVER_MSG
                                      WHERE   (MsgId > 0)
                                      GROUP BY FromUserName))) AS a_1 LEFT OUTER JOIN
                          (SELECT   FromUserName, MAX(Id) AS id, MAX(CreateTime) AS createTime
                          FROM      dbo.T_CUS_SERVER_MSG AS T_CUS_SERVER_MSG_1
                          GROUP BY FromUserName) AS m ON m.FromUserName = a_1.FWXOPENID LEFT OUTER JOIN
                          dbo.T_CUS_SERVER_MSG AS m1 ON m1.Id = m.id
                          ORDER BY m1.CreateTime DESC";
            return session.CreateSQLQuery(sql)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }

        /// <summary>
        /// 查看聊天记录的第一条消息的时间
        /// </summary>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_SelectMSGFirst(string OPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   MIN(T_CUS_SERVER_MSG.CreateTime) AS createtime, derivedtbl_1.FNAME, derivedtbl_1.FWXOPENID, 
                derivedtbl_1.Expr2, derivedtbl_1.Expr4, derivedtbl_1.FPKID
FROM      T_CUS_SERVER_MSG RIGHT OUTER JOIN
                    (SELECT   derivedtbl_1_1.FPKID, derivedtbl_1_1.FNAME, derivedtbl_1_1.FPERMISSIONS, 
                                     derivedtbl_1_1.FWXOPENID, derivedtbl_2.FPKID AS Expr1, derivedtbl_2.FNAME AS Expr2, 
                                     derivedtbl_2.FPERMISSIONS AS Expr3, derivedtbl_2.FWXOPENID AS Expr4
                     FROM      T_ESS_CHANNELSTAFF_RelationShip INNER JOIN
                                         (SELECT   T_ESS_CHANNELSTAFF_L.FPKID, T_ESS_CHANNELSTAFF_L.FNAME, 
                                                          A_ROLE.FPERMISSIONS, T_ESS_CHANNELSTAFF.FWXOPENID
                                          FROM      A_ROLE INNER JOIN
                                                          T_ESS_CHANNELSTAFF_L ON A_ROLE.FID = T_ESS_CHANNELSTAFF_L.FROLEID INNER JOIN
                                                          T_ESS_CHANNELSTAFF ON 
                                                          T_ESS_CHANNELSTAFF_L.FPKID = T_ESS_CHANNELSTAFF.FID
                                          WHERE   (A_ROLE.FPERMISSIONS = '1')) derivedtbl_1_1 ON 
                                     T_ESS_CHANNELSTAFF_RelationShip.StaffID = derivedtbl_1_1.FPKID INNER JOIN
                                         (SELECT   T_ESS_CHANNELSTAFF_L_1.FPKID, T_ESS_CHANNELSTAFF_L_1.FNAME, 
                                                          A_ROLE_1.FPERMISSIONS, T_ESS_CHANNELSTAFF_1.FWXOPENID
                                          FROM      A_ROLE A_ROLE_1 INNER JOIN
                                                          T_ESS_CHANNELSTAFF_L T_ESS_CHANNELSTAFF_L_1 ON 
                                                          A_ROLE_1.FID = T_ESS_CHANNELSTAFF_L_1.FROLEID INNER JOIN
                                                          T_ESS_CHANNELSTAFF T_ESS_CHANNELSTAFF_1 ON 
                                                          T_ESS_CHANNELSTAFF_L_1.FPKID = T_ESS_CHANNELSTAFF_1.FID) derivedtbl_2 ON 
                                     T_ESS_CHANNELSTAFF_RelationShip.CustomerID = derivedtbl_2.FPKID
                     WHERE   (derivedtbl_2.FWXOPENID = '" + OPENID + @"')) derivedtbl_1 ON 
                T_CUS_SERVER_MSG.ToUserName = derivedtbl_1.Expr4 AND 
                T_CUS_SERVER_MSG.FromUserName = derivedtbl_1.FWXOPENID
GROUP BY derivedtbl_1.FNAME, derivedtbl_1.FWXOPENID, derivedtbl_1.Expr2, derivedtbl_1.Expr4, derivedtbl_1.FPKID";
            return session.CreateSQLQuery(sql)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }

        /// <summary>
        /// 查看聊天记录的最后一条的时间
        /// </summary>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_SelectMSGLast(string OPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   MAX(T_CUS_SERVER_MSG.CreateTime) AS createtime, derivedtbl_1.FNAME, derivedtbl_1.FWXOPENID, 
                derivedtbl_1.Expr2, derivedtbl_1.Expr4, derivedtbl_1.FPKID
FROM      T_CUS_SERVER_MSG RIGHT OUTER JOIN
                    (SELECT   derivedtbl_1_1.FPKID, derivedtbl_1_1.FNAME, derivedtbl_1_1.FPERMISSIONS, 
                                     derivedtbl_1_1.FWXOPENID, derivedtbl_2.FPKID AS Expr1, derivedtbl_2.FNAME AS Expr2, 
                                     derivedtbl_2.FPERMISSIONS AS Expr3, derivedtbl_2.FWXOPENID AS Expr4
                     FROM      T_ESS_CHANNELSTAFF_RelationShip INNER JOIN
                                         (SELECT   T_ESS_CHANNELSTAFF_L.FPKID, T_ESS_CHANNELSTAFF_L.FNAME, 
                                                          A_ROLE.FPERMISSIONS, T_ESS_CHANNELSTAFF.FWXOPENID
                                          FROM      A_ROLE INNER JOIN
                                                          T_ESS_CHANNELSTAFF_L ON A_ROLE.FID = T_ESS_CHANNELSTAFF_L.FROLEID INNER JOIN
                                                          T_ESS_CHANNELSTAFF ON 
                                                          T_ESS_CHANNELSTAFF_L.FPKID = T_ESS_CHANNELSTAFF.FID
                                          WHERE   (A_ROLE.FPERMISSIONS = '1')) derivedtbl_1_1 ON 
                                     T_ESS_CHANNELSTAFF_RelationShip.StaffID = derivedtbl_1_1.FPKID INNER JOIN
                                         (SELECT   T_ESS_CHANNELSTAFF_L_1.FPKID, T_ESS_CHANNELSTAFF_L_1.FNAME, 
                                                          A_ROLE_1.FPERMISSIONS, T_ESS_CHANNELSTAFF_1.FWXOPENID
                                          FROM      A_ROLE A_ROLE_1 INNER JOIN
                                                          T_ESS_CHANNELSTAFF_L T_ESS_CHANNELSTAFF_L_1 ON 
                                                          A_ROLE_1.FID = T_ESS_CHANNELSTAFF_L_1.FROLEID INNER JOIN
                                                          T_ESS_CHANNELSTAFF T_ESS_CHANNELSTAFF_1 ON 
                                                          T_ESS_CHANNELSTAFF_L_1.FPKID = T_ESS_CHANNELSTAFF_1.FID) derivedtbl_2 ON 
                                     T_ESS_CHANNELSTAFF_RelationShip.CustomerID = derivedtbl_2.FPKID
                     WHERE   (derivedtbl_2.FWXOPENID = '" + OPENID + @"')) derivedtbl_1 ON 
                T_CUS_SERVER_MSG.ToUserName = derivedtbl_1.Expr4 AND 
                T_CUS_SERVER_MSG.FromUserName = derivedtbl_1.FWXOPENID
GROUP BY derivedtbl_1.FNAME, derivedtbl_1.FWXOPENID, derivedtbl_1.Expr2, derivedtbl_1.Expr4, derivedtbl_1.FPKID";
            return session.CreateSQLQuery(sql)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }
        /// <summary>
        /// 查询所关联的客服id及小程序openid
        /// </summary>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public dynamic ZXKH_SelectShip(string OPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT dbo.T_ESS_CHANNELSTAFF.FWXOPENID, dbo.T_ESS_CHANNELSTAFF.XCXOPENID,
                T_ESS_CHANNELSTAFF_1.FWXOPENID AS staffid, T_ESS_CHANNELSTAFF_1.XCXOPENID AS staffxcxid
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON
                dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID = T_ESS_CHANNELSTAFF_1.FID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.FWXOPENID = :p1)";
            var staff = session.CreateSQLQuery(sql)
            .SetParameter("p1", OPENID)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>()
            .FirstOrDefault();
            return staff;
        }
        

        /// <summary>
        /// 通过客户openid获取到客服的openid  暂时没有用到
        /// </summary>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public IList<dynamic> ZXKH_SelectKH(string OPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"select FWXOPENID from T_ESS_CHANNELSTAFF where FID = (select StaffID from T_ESS_CHANNELSTAFF_RelationShip where CustomerID = (select fid from T_ESS_CHANNELSTAFF where FWXOPENID=:p1))";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", OPENID)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }

        /// <summary>
        /// 通过群组编号获取群组所有成员头像,按加入群时间先后顺序
        /// </summary>
        /// <param name="OPENID"></param>
        public IList<dynamic> ZXKH_GroupManagement(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT     TOP (100) PERCENT dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, dbo.T_ESS_CHANNELSTAFF.KHNAME
                      FROM         dbo.T_ESS_CHANNELSTAFF_GROUPSHIP INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF_GROUP ON dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF_AVATAR ON dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF ON dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID = dbo.T_ESS_CHANNELSTAFF.FID
                      WHERE     (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = :p1)
                      ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.JoinTime";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", groupno)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }
        /// <summary>
        /// 通过群组编号获取群组的基础信息
        /// </summary>
        /// <param name="OPENID"></param>
        public IList<dynamic> ZXKH_GroupInformation(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   GroupName, GroupQRcode, GroupRemarks, GroupNo, UserFID, createtime, GroupImgBase64
                           FROM      T_ESS_CHANNELSTAFF_GROUP
                           WHERE   (GroupNo = :p1)";
            return session.CreateSQLQuery(sql)
            .SetParameter("p1", groupno)
            .SetResultTransformer(new AliasToEntityMapResultTransformer())
            .List<dynamic>();
        }
        /// <summary>
        /// 通过群组编号修改群名称
        /// </summary>
        /// <param name="OPENID"></param>
        public void ZXKH_EditGroupName(string groupname, string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"update T_ESS_CHANNELSTAFF_GROUP set GroupName=:p1 where GroupNo = :p2";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", groupname)
                   .SetParameter("p2", groupno)
                   .ExecuteUpdate();
        }
        public dynamic ZXKH_BoolGroupUse(string useropenid, string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT     dbo.T_ESS_CHANNELSTAFF_GROUP.GroupName, dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo, dbo.T_ESS_CHANNELSTAFF.FID,                     dbo.T_ESS_CHANNELSTAFF.XCXOPENID, 
                      dbo.T_ESS_CHANNELSTAFF.KHNAME
                      FROM         dbo.T_ESS_CHANNELSTAFF_GROUPSHIP INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF_GROUP ON dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo INNER JOIN
                      dbo.T_ESS_CHANNELSTAFF ON dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID = dbo.T_ESS_CHANNELSTAFF.FID
                      WHERE     (dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = :p2) AND (dbo.T_ESS_CHANNELSTAFF.XCXOPENID = :p1)";
            var result = session.CreateSQLQuery(sql)
                   .SetParameter("p1", useropenid)
                   .SetParameter("p2", groupno)
                   .SetResultTransformer(new AliasToEntityMapResultTransformer())
                   .List<dynamic>()
                   .FirstOrDefault();
            return result;
        }
        

        /// <summary>
        /// 群组踢人
        /// </summary>
        /// <param name="OPENID"></param>
        public void ZXKH_GroupRemove(string staffid, string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"DELETE FROM T_ESS_CHANNELSTAFF_GROUPSHIP WHERE GroupNo = :p1 AND UserFID = :p2";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", groupno)
                   .SetParameter("p2", staffid)
                   .ExecuteUpdate();
        }
        /// <summary>
        /// 群组踢人
        /// </summary>
        /// <param name="OPENID"></param>
        public void ZXKH_GroupAdd(string staffid, string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"INSERT INTO  T_ESS_CHANNELSTAFF_GROUPSHIP VALUES(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()))";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", groupno)
                   .SetParameter("p2", staffid)
                   .ExecuteUpdate();
        }
        /// <summary>
        /// 群组作废
        /// </summary>
        /// <param name="OPENID"></param>
        public void ZXKH_GroupVoid(string groupno)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"update T_ESS_CHANNELSTAFF_GROUP set GroupState = '作废' where GroupNo = :p1";
            session.CreateSQLQuery(sql)
                   .SetParameter("p1", groupno)
                   .ExecuteUpdate();
        }
        public dynamic ZXKH_OpenidToName(string XCXOPENID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   KHNAME
                           FROM      dbo.T_ESS_CHANNELSTAFF
                           WHERE   (XCXOPENID = :P1)";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", XCXOPENID)
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
        public dynamic ZXKH_QueryUserWebSocket(string userId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT * FROM T_ESS_CHANNELSTAFF_WebSocket WHERE STAFFID = :P1";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", userId)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;
        }
        public void ZXKH_AddUserWebSocket(string userId,string connectionID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"insert into T_ESS_CHANNELSTAFF_WebSocket values(:P1,:P2)";

            session
                .CreateSQLQuery(sql)
                .SetParameter("P1", userId)
                .SetParameter("P2", connectionID)
                .ExecuteUpdate();
        }
        public void ZXKH_EditUserWebSocket(string userId, string connectionID)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"update T_ESS_CHANNELSTAFF_WebSocket set ConnectionID =:P2  where STAFFID = :P1";

            session
                .CreateSQLQuery(sql)
                .SetParameter("P1", userId)
                .SetParameter("P2", connectionID)
                .ExecuteUpdate();
        }
        /// <summary>
        /// 通过小程序openid获取用户图像
        /// </summary>
        /// <param name="xcxOpenid"></param>
        /// <returns></returns>
        public dynamic ZXKH_QueryPicture(string xcxOpenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, dbo.T_ESS_CHANNELSTAFF.KHNAME
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                WHERE   (dbo.T_ESS_CHANNELSTAFF.XCXOPENID = :P1)";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", xcxOpenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;
        }
        /// <summary>
        /// 通过小程序openid获取用户fid
        /// </summary>
        /// <param name="xcxOpenid"></param>
        /// <returns></returns>
        public dynamic ZXKH_QueryFID(string xcxOpenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   FID
                           FROM      dbo.T_ESS_CHANNELSTAFF
                           WHERE   (XCXOPENID = :P1)";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", xcxOpenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            return staff;
        }
        /// <summary>
        /// 通过群编号获取群用户下所有fid
        /// </summary>
        /// <param name="xcxOpenid"></param>
        /// <returns></returns>
        public dynamic ZXKH_GroupQueryFID(string xcxOpenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID
                FROM      dbo.T_ESS_CHANNELSTAFF_GROUP INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
                dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo
                WHERE   (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = :P1)";

            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", xcxOpenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
            return staff;
        }
        public void ZXKH_Message_top(string xcxopenid, string xcxopenid_top)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"insert into T_CUS_SERVER_MSG_TOP values(:p1,:p2,DATEDIFF(second, '1970-01-01 08:00:00', GETDATE()))";

            session
                .CreateSQLQuery(sql)
                .SetParameter("p1", xcxopenid)
                .SetParameter("p2", xcxopenid_top)
                .ExecuteUpdate();
        }

        public string bbb()
        {
            return "以上新加";
        }

        /// <summary>
        /// pc用户列表
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> pc_QueryCustomers()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"select * from (
                SELECT  ROW_NUMBER()   
                over   
                (PARTITION By T_ESS_CHANNELSTAFF_1.FID order by dbo.T_CUS_SERVER_MSG.CreateTime DESC) as rowId, T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, 
                dbo.T_CUS_SERVER_MSG.[Content] AS [content], dbo.T_CUS_SERVER_MSG.CreateTime as createtime
                FROM      dbo.T_ESS_CHANNELSTAFF_AVATAR INNER JOIN
                dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 ON 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID = T_ESS_CHANNELSTAFF_1.FID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID ON 
                T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID LEFT OUTER JOIN
                dbo.T_CUS_SERVER_MSG ON 
                T_ESS_CHANNELSTAFF_1.XCXOPENID = dbo.T_CUS_SERVER_MSG.XCXFromOpenId AND 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID = dbo.T_CUS_SERVER_MSG.XCXToOpenId
                WHERE   (dbo.T_ESS_CHANNELSTAFF.XCXOPENID in(SELECT   a.XCXOPENID
FROM      dbo.T_ESS_CHANNELSTAFF AS a LEFT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR AS b ON a.FID = b.STAFFID LEFT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF_L AS c ON c.FID = a.FID LEFT OUTER JOIN
                dbo.A_ROLE AS d ON d.FID = c.FROLEID
WHERE   (d.FNAME = '客服')) )
                GROUP BY T_ESS_CHANNELSTAFF_1.FID, T_ESS_CHANNELSTAFF_1.KHNAME, dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE, 
                T_ESS_CHANNELSTAFF_1.XCXOPENID, T_ESS_CHANNELSTAFF_1.FWXOPENID, dbo.T_CUS_SERVER_MSG.[Content], 
                dbo.T_CUS_SERVER_MSG.CreateTime
                ) t
                where rowid <= 1 and CreateTime <>''";
            return session.CreateSQLQuery(sql)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }
        /// <summary>
        /// pc群组列表所有正常状态的
        /// </summary>
        /// <returns></returns>
        public IList<dynamic> pc_ZXKH_ALLGroupList()
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT   TOP (100) PERCENT GroupID, GroupName, GroupQRcode, GroupRemarks, GroupNo as xcxopenid, UserFID, createtime, 
                            GroupImgBase64 as picUrl, GroupState
                            FROM      dbo.T_ESS_CHANNELSTAFF_GROUP
                            WHERE   (GroupState = N'正常')
                            ORDER BY createtime DESC";
            return session.CreateSQLQuery(sql)
           .SetResultTransformer(new AliasToEntityMapResultTransformer())
           .List<dynamic>();
        }

        public string pc_kf_Lastgroup(string XCXFromOpenId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   TOP (1) PERCENT derivedtbl_1.XCXFromOpenId
                        FROM      (SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.XCXToOpenId, 
                                                         dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.CreateTime
                                         FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                                                         dbo.T_ESS_CHANNELSTAFF ON 
                                                         dbo.T_CUS_SERVER_MSG.XCXFromOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID INNER JOIN
                                                         dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                                                         dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
                                         WHERE   (dbo.T_CUS_SERVER_MSG.XCXToOpenId = :P1)) 
                                        AS derivedtbl_1 INNER JOIN
                                            (SELECT   TOP (1) dbo.A_ROLE.FPERMISSIONS, T_ESS_CHANNELSTAFF_2.XCXOPENID
                                             FROM      dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 INNER JOIN
                                                             dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                                                             T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID RIGHT OUTER JOIN
                                                             dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_2 INNER JOIN
                                                             dbo.T_ESS_CHANNELSTAFF_L ON 
                                                             T_ESS_CHANNELSTAFF_2.FID = dbo.T_ESS_CHANNELSTAFF_L.FID INNER JOIN
                                                             dbo.A_ROLE ON dbo.T_ESS_CHANNELSTAFF_L.FROLEID = dbo.A_ROLE.FID ON 
                                                             dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID = T_ESS_CHANNELSTAFF_2.FID
                                             WHERE   (dbo.A_ROLE.FPERMISSIONS = 1)) AS derivedtbl_2 ON 
                                        derivedtbl_1.XCXFromOpenId = derivedtbl_2.XCXOPENID
                        ORDER BY derivedtbl_1.XCXToOpenId DESC";
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", XCXFromOpenId)
                .List<string>()
                .FirstOrDefault();
            return staff;
        }

        public IList<dynamic> pc_Que(string toGroup)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            var sql = @"SELECT   TOP (1) PERCENT dbo.T_ESS_CHANNELSTAFF.FID, dbo.T_ESS_CHANNELSTAFF.KHNAME, 
                dbo.T_ESS_CHANNELSTAFF.XCXOPENID, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo as xcxopenid, 
                dbo.T_ESS_CHANNELSTAFF_GROUP.GroupName, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupImgBase64, 
                dbo.T_ESS_CHANNELSTAFF_GROUP.createtime, dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState
                FROM      dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.UserFID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_GROUP ON 
                dbo.T_ESS_CHANNELSTAFF_GROUPSHIP.GroupNo = dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo
                WHERE   (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupNo = :p1) AND 
                (dbo.T_ESS_CHANNELSTAFF_GROUP.GroupState = N'正常')
                ORDER BY dbo.T_ESS_CHANNELSTAFF_GROUP.createtime DESC";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", toGroup)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        /// <summary>
        /// 查询客户信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <returns></returns>
        public IList<dynamic> pc_QueryCustomerInfo(string wxopenid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //!返回的字段必须小写
            string sql = @"SELECT a.fid,a.khname,b.picture,a.fwxopenid FROM dbo.T_ESS_CHANNELSTAFF a 
LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR b ON a.FID=b.STAFFID 
WHERE a.FWXOPENID = :p1";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        public IList<CustomerServiceMessage> pc_QueryCustomerMessage(string wxopenid, string openid, int page, int limit)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            //var sql = @"SELECT a.Id, a.MsgId, a.ToUserName, a.FromUserName, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId,a.XCXFromOpenId,a.XCXToOpenId
            //            FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.ToUserName, ISNULL(cs.KHNAME, t.FromUserName) FromUserName, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId,t.XCXFromOpenId,t.XCXToOpenId
            //                 FROM(SELECT *
            //                      FROM dbo.T_CUS_SERVER_MSG
            //                      WHERE XCXFromOpenId= :p1
            //                      UNION
            //                      SELECT *
            //                      FROM dbo.T_CUS_SERVER_MSG
            //                      WHERE XCXToOpenId=:p1) t
            //                     LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.FromUserName
            //                     LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
            //            WHERE a.xh> :p2 AND xh<= :p3 ORDER BY Id";
            string sql = @"SELECT a.Id, a.MsgId, a.XCXToOpenId, a.XCXFromOpenId, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId, a.Format, a.Recognition
        FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.XCXToOpenId, ISNULL(cs.KHNAME, t.XCXFromOpenId) XCXFromOpenId, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId, t.Format, t.Recognition
        FROM(
           SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXFromOpenId='-1' and XCXToOpenId= :p0
          UNION
            SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXFromOpenId= :p0 and XCXToOpenId= '-1'
          UNION
          SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXFromOpenId=:p1 and XCXToOpenId= :p2
          UNION
          SELECT *
          FROM dbo.T_CUS_SERVER_MSG
          WHERE XCXToOpenId=:p3 and XCXFromOpenId=:p4) t
         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.XCXFromOpenId
         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
        WHERE a.xh>:p5 AND xh<=:p6 ORDER BY Id";
            return session.CreateSQLQuery(sql)
                .SetParameter("p0", wxopenid)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", openid)
                .SetParameter("p3", wxopenid)
                .SetParameter("p4", openid)
                .SetParameter("p5", (page - 1) * limit)
                .SetParameter("p6", page * limit)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CustomerServiceMessage)))
                .List<CustomerServiceMessage>();
        }

        public IList<dynamic> pc_ZXKH_QueryGroupMsg(string wxopenid, int page, int limit)
        {
            //WxSendMessage
            ISession session = NHSessionProvider.GetCurrentSession();
            
            string sql = @"SELECT a.Id, a.MsgId, a.XCXToOpenId, a.XCXFromOpenId, a.CreateTime, a.MsgType, a.Content, a.Title, a.AppId, a.PicUrl, a.PagePath, a.MediaId, a.ThumbUrl, a.ThumbMediaId,a.PICTURE,a.FromUserName
        FROM(SELECT ROW_NUMBER() OVER (ORDER BY t.Id DESC) xh, t.Id, t.MsgId, t.XCXToOpenId, ISNULL(cs.KHNAME, t.XCXFromOpenId) XCXFromOpenId, t.CreateTime, t.MsgType, t.Content, t.Title, t.AppId, t.PicUrl, t.PagePath, t.MediaId, ca.PICTURE ThumbUrl, t.ThumbMediaId,t.PICTURE,t.FromUserName
     FROM(SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.CreateTime, 
                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE,dbo.T_CUS_SERVER_MSG.FromUserName
FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
WHERE   (dbo.T_CUS_SERVER_MSG.XCXFromOpenId = :p1)
          UNION
          SELECT   dbo.T_CUS_SERVER_MSG.Id, dbo.T_CUS_SERVER_MSG.MsgId, dbo.T_CUS_SERVER_MSG.XCXToOpenId, 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId, dbo.T_CUS_SERVER_MSG.CreateTime, 
                dbo.T_CUS_SERVER_MSG.MsgType, dbo.T_CUS_SERVER_MSG.[Content], dbo.T_CUS_SERVER_MSG.Title, 
                dbo.T_CUS_SERVER_MSG.AppId, dbo.T_CUS_SERVER_MSG.PicUrl, dbo.T_CUS_SERVER_MSG.PagePath, 
                dbo.T_CUS_SERVER_MSG.MediaId, dbo.T_CUS_SERVER_MSG.ThumbUrl, dbo.T_CUS_SERVER_MSG.ThumbMediaId, 
                dbo.T_ESS_CHANNELSTAFF_AVATAR.PICTURE,dbo.T_CUS_SERVER_MSG.FromUserName
FROM      dbo.T_CUS_SERVER_MSG INNER JOIN
                dbo.T_ESS_CHANNELSTAFF ON 
                dbo.T_CUS_SERVER_MSG.XCXFromOpenId = dbo.T_ESS_CHANNELSTAFF.XCXOPENID INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_AVATAR ON 
                dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_AVATAR.STAFFID
WHERE   (dbo.T_CUS_SERVER_MSG.XCXToOpenId = :p1)) t
         LEFT JOIN T_ESS_CHANNELSTAFF cs ON CAST(cs.FID AS VARCHAR(20))=t.XCXFromOpenId
         LEFT JOIN T_ESS_CHANNELSTAFF_AVATAR ca ON ca.STAFFID=cs.FID) a
WHERE a.xh>:p2 AND xh<=:p3 ORDER BY Id";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .SetParameter("p2", (page - 1) * limit)
                .SetParameter("p3", page * limit)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        /// <summary>
        /// 查询客户关联的客服
        /// </summary>
        /// <param name="wxopenid">客户的小程序openid</param>
        /// <returns></returns>
        public string relation(string wxopenid)
        {
            //WxSendMessage
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql = @"SELECT   
                T_ESS_CHANNELSTAFF_1.XCXOPENID
FROM      dbo.T_ESS_CHANNELSTAFF AS T_ESS_CHANNELSTAFF_1 INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_RelationShip ON 
                T_ESS_CHANNELSTAFF_1.FID = dbo.T_ESS_CHANNELSTAFF_RelationShip.StaffID RIGHT OUTER JOIN
                dbo.T_ESS_CHANNELSTAFF INNER JOIN
                dbo.T_ESS_CHANNELSTAFF_L ON dbo.T_ESS_CHANNELSTAFF.FID = dbo.T_ESS_CHANNELSTAFF_L.FID INNER JOIN
                dbo.A_ROLE ON dbo.T_ESS_CHANNELSTAFF_L.FROLEID = dbo.A_ROLE.FID ON 
                dbo.T_ESS_CHANNELSTAFF_RelationShip.CustomerID = dbo.T_ESS_CHANNELSTAFF.FID
WHERE   (dbo.T_ESS_CHANNELSTAFF.XCXOPENID = :p1)";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", wxopenid)
                .List<string>().FirstOrDefault();
        }

        public IList<dynamic> pc_GetKfSelect(string id)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT  A.FWXOPENID,A.FID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID

                   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON(TTL.FTYPEID = L.FCHANNELTYPEID AND TTL.FLOCALEID = 2052)  WHERE A.FENABLE = 1  AND B.FJOB = '客服' AND A.FID <> :p1";
            return session.CreateSQLQuery(sql)
                .SetParameter("p1", id)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>();
        }

        public void kfRelation(string userId, string fid)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_KFgl]( [zfid],[ffid])VALUES(:P1,:P2)";
            session
                 .CreateSQLQuery(sql)
                 .SetParameter("P1", Convert.ToInt32(userId))
                 .SetParameter("P2", Convert.ToInt32(fid))
                 .ExecuteUpdate();
        }

        public string GetTel(string userId)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql = @"SELECT FMOBILE
                        FROM T_ESS_CHANNELSTAFF
                        WHERE(FID = :p1)";
             return session.CreateSQLQuery(sql)
                .SetParameter("p1", userId)
                .List<string>().FirstOrDefault();
        }
    }
}
