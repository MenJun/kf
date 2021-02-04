using NHibernate;
using NHibernate.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Common.Utils;

namespace Common.Filter
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionForK3Attribute : ActionFilterAttribute
    {
        //private ITransaction currentTransaction;
        
        private readonly System.Data.IsolationLevel _isolationLevel;

        public TransactionForK3Attribute(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            _isolationLevel = isolationLevel;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (CurrentSessionContext.HasBind(NHK3SessionProvider.SessionFactory) == false)
            {
                CurrentSessionContext.Bind(NHK3SessionProvider.SessionFactory.OpenSession());
            }

            if ((NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.IsActive == false))
            {
                NHK3SessionProvider.SessionFactory.GetCurrentSession().BeginTransaction(_isolationLevel);
            }
            //currentTransaction = NHSessionProvider.GetCurrentSession().BeginTransaction(_isolationLevel);
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //if (currentTransaction.IsActive)
            //{
            //    if(actionExecutedContext.Exception == null)
            //    {
            //        currentTransaction.Commit();
            //    }
            //    else
            //    {
            //        currentTransaction.Rollback();
            //    }
            //}

            if ((NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.IsActive == true)
                && (NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.WasCommitted == false)
                 && (NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.WasRolledBack == false))
            {
                if (actionExecutedContext.Exception == null)
                {
                    try
                    {
                        NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.Commit();
                    }
                    catch (System.Exception)
                    {
                        NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.Rollback();
                        throw;
                    }
                    
                }
                else
                {
                    NHK3SessionProvider.SessionFactory.GetCurrentSession().Transaction.Rollback();
                }
            }
            CurrentSessionContext.Unbind(NHK3SessionProvider.SessionFactory).Dispose();
        }
    }
}
