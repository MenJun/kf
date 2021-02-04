using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Entity
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:55:00
	*描  述  ：
	***********************************************************************/
    public class AuthorityModule
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
        public virtual string Icon
        {
            get;
            set;
        }
        public virtual bool Enable
        {
            get;
            set;
        }
        public virtual bool Checked
        {
            get;
            set;
        }
        public virtual IList<AuthorityPage> AuthorityPages
        {
            get;
            set;
        }
        public AuthorityModule()
        {
            AuthorityPages = new List<AuthorityPage>();
        }
    }
}
