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
    public class TransactionAttribute : ActionFilterAttribute
    {
        //private ITransaction currentTransaction;
        
        private readonly System.Data.IsolationLevel _isolationLevel;

        public TransactionAttribute(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            _isolationLevel = isolationLevel;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (CurrentSessionContext.HasBind(NHSessionProvider.SessionFactory) == false)
            {
                CurrentSessionContext.Bind(NHSessionProvider.SessionFactory.OpenSession());
            }

            if ((NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.IsActive == false))
            {
                NHSessionProvider.SessionFactory.GetCurrentSession().BeginTransaction(_isolationLevel);
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

            if ((NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.IsActive == true)
                && (NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.WasCommitted == false)
                 && (NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.WasRolledBack == false))
            {
                if (actionExecutedContext.Exception == null)
                {
                    try
                    {
                        NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.Commit();
                    }
                    catch (System.Exception)
                    {
                        NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.Rollback();
                        throw;
                    }
                    
                }
                else
                {
                    NHSessionProvider.SessionFactory.GetCurrentSession().Transaction.Rollback();
                }
            }
            CurrentSessionContext.Unbind(NHSessionProvider.SessionFactory).Dispose();
        }
    }
}
