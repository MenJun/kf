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
	*创建时间：2019/10/22 10:08:28
	*描  述  ：
	***********************************************************************/
    public class RoleModule
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual AuthorityModule AuthorityModule
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
    }
}
