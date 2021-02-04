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

using System.Configuration;
using Api.Model.DOM;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;

namespace Common.Utils
{
    /// <summary>
    /// A5Service
    /// </summary>
    public sealed class NHSessionProvider
    {
        private static readonly string conStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;

        static NHSessionProvider()
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        private NHSessionProvider()
        {
        }

        private static void BindSession()
        {
            if (!CurrentSessionContext.HasBind(SessionFactory))
            {
                CurrentSessionContext.Bind(SessionFactory.OpenSession());
            }
        }

        public static ISession GetCurrentSession()
        {
            BindSession();
            return SessionFactory.GetCurrentSession();
        }

        public static ISessionFactory SessionFactory { get; } = Fluently.Configure().Database(
                       MsSqlConfiguration.MsSql2008
                         //.Raw("connection.isolation", "isolation_level")
                         .ConnectionString(conStr).ShowSql()
                       ).Mappings(m => m.FluentMappings.AddFromAssemblyOf<ARole_Mapper>())
            .ExposeConfiguration(c => {
                c.SetProperty(Environment.CommandTimeout, "120");
                c.SetProperty(Environment.CurrentSessionContextClass, "web");
            })
            .BuildSessionFactory();
    }
    /// <summary>
    /// K3_Service
    /// </summary>
    public sealed class NHK3SessionProvider
    {
        private static readonly string connK3 = ConfigurationManager.ConnectionStrings["connK3"].ConnectionString;

        static NHK3SessionProvider()
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        private NHK3SessionProvider()
        {
        }
        private static void BindSession()
        {
            if (!CurrentSessionContext.HasBind(SessionFactory))
            {
                CurrentSessionContext.Bind(SessionFactory.OpenSession());
            }
        }

        public static ISession GetCurrentSession()
        {
            BindSession();
            return SessionFactory.GetCurrentSession();
        }

        public static ISessionFactory SessionFactory { get; } = Fluently.Configure().Database(
                       MsSqlConfiguration.MsSql2008
                         //.Raw("connection.isolation", "isolation_level")
                         .ConnectionString(connK3).ShowSql()
                       ).Mappings(m => m.FluentMappings.AddFromAssemblyOf<ARole_Mapper>())
            .ExposeConfiguration(c => {
                c.SetProperty(Environment.CommandTimeout, "120");
                c.SetProperty(Environment.CurrentSessionContextClass, "web");
            })
            .BuildSessionFactory();

    }

   
}
