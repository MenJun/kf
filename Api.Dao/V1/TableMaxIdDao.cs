using System;
using System.Linq;
using Common.Utils;
using NHibernate;

namespace Api.Dao.V1
{
    public class TableMaxIdDao
    {
        //获取最大号
        public long QueryMaxId(string tableName)
        {
            ISession session = NHSessionProvider.GetCurrentSession();

            string sql1 = $"INSERT INTO Z_{tableName} VALUES('')";
            string sql2 = $"SELECT ID FROM Z_{tableName}";

            string sql3 = $"DELETE FROM Z_{tableName}";

            session.CreateSQLQuery(sql1).ExecuteUpdate();
            long id = session.CreateSQLQuery(sql2).List<Int64>().FirstOrDefault();
            session.CreateSQLQuery(sql3).ExecuteUpdate();

            return id;
        }
        //获取单号最大编号位数

        public int QueryMaxFnumbe(string tableName, string years, string months)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql2 = $"SELECT isnull(right(max(FNUMBE),4),0)+1 FROM {tableName} where left(right(FNUMBE,10),4)={years}  and left(right(FNUMBE,6),2) ={months}   ";
            int id = session.CreateSQLQuery(sql2).List<int>().FirstOrDefault();
            return id;
        }

        public int QueryMaxFnumber(string tableName, string years, string months)
        {
            ISession session = NHSessionProvider.GetCurrentSession();
            string sql2 = $"SELECT isnull(right(max(FNUMBER),4),0)+1 FROM {tableName} where left(right(FNUMBER,10),4)={years}  and left(right(FNUMBER,6),2) ={months}   ";
            int id = session.CreateSQLQuery(sql2).List<int>().FirstOrDefault();
            return id;
        }
    }
}
