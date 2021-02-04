using Api.Model.BO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Api.Dao.V1;
using Common.Utils;
using Unity.Attributes;

namespace Common.Filter
{
    public class ActionPermission : ActionFilterAttribute
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get;
            set;
        }
        /// <summary>
        /// 权限简称
        /// </summary>
        public string PerName
        {
            get;
            set;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            var token = request.Headers.GetValues("Authorization").FirstOrDefault();
            token = token.Substring(token.IndexOf(" ") + 1);

            JwtPayload payload = JsonConvert.DeserializeObject<JwtPayload>(JwtHelper.VerifyToken(token).ToString());
            if (payload.UserName != UserName)
            {
                var staff = new StaffDao().DetailEntryById(payload.UserId).FirstOrDefault();
                IList<string> pers = new BaseDataDao().QueryRoleHasPermissions(staff.FROLEID);
                if (!pers.Contains(PerName))
                {
                    throw new System.Exception("no action permission");
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
