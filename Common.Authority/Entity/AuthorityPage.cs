using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Entity
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:53:36
	*描  述  ：
	***********************************************************************/
    public class AuthorityPage
    {
        public virtual int Id
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

        public virtual bool NotAuth
        {
            get;
            set;
        }

        public virtual AuthorityModule AuthorityModule
        {
            get;
            set;
        }

        public virtual IList<Permission> Permissions
        {
            get;
            set;
        }

        public AuthorityPage()
        {
            Permissions = new List<Permission>();
        }
    }
}
