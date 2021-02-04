using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Entity
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:40:30
	*描  述  ：
	***********************************************************************/
    public class Role
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual int StoreId
        {
            get;
            set;
        }
        public virtual string Name
        {
            get;
            set;
        }
        public virtual bool Enable
        {
            get;
            set;
        }
        public virtual IList<RoleModule> RoleModules
        {
            get;
            set;
        }
        [JsonIgnore]
        public virtual IList<RolePermission> RolePermissions
        {
            get;
            set;
        }

        public Role()
        {
            RoleModules = new List<RoleModule>();
            RolePermissions = new List<RolePermission>();
        }

    }
}
