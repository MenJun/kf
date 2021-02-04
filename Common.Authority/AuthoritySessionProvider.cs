// *************************************************************
//
// 文件名(File Name)：SessionProvider
//
// 功能描述(Description)：hibernate服务类
// 
// 作者(Author)：陈光华
//
// 创建日期(Create Date)：2017/12/25 17:13:41
//
// 修改记录(Revision History)：
//		R1：
//			修改作者：
//			修改日期：
//			修改描述：
//
//		R2：
//			修改作者：
//			修改日期：
//			修改描述：
//
// *************************************************************
using Common.Authority.EntityMap;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;
using System.Configuration;

namespace Common.Authority
{
    /// <summary>
    /// A5Service
    /// </summary>
    public sealed class AuthoritySessionProvider
    {
        private static readonly string conStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;

        static AuthoritySessionProvider()
        {
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        private AuthoritySessionProvider()
        {
        }

        public static ISession GetSession()
        {
            return SessionFactory.OpenSession();
        }

        public static IStatelessSession GetStatelessSession()
        {
            return SessionFactory.OpenStatelessSession();
        }

        public static ISessionFactory SessionFactory { get; } = Fluently.Configure().Database(
                       MsSqlConfiguration.MsSql2008
                         //.Raw("connection.isolation", "isolation_level")
                         .ConnectionString(conStr).ShowSql()
                       ).Mappings(m => m.FluentMappings.AddFromAssemblyOf<RoleMap>())
            .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(true,true))
            //.ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(true, true, false))
            .BuildSessionFactory();
    }
}
