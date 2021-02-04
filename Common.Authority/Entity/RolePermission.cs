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
	*创建时间：2019/10/22 10:15:42
	*描  述  ：
	***********************************************************************/
    public class RolePermission
    {
        public virtual int Id
        {
            get;
            set;
        }
        [JsonIgnore]
        public virtual Role Role
        {
            get;
            set;
        }
        public virtual AuthorityPage AuthorityPage
        {
            get;
            set;
        }

        public virtual Permission Permission
        {
            get;
            set;
        }
    }
}
