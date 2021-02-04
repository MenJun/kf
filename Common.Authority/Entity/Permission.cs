using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Entity
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:44:29
	*描  述  ：
	***********************************************************************/
    public class Permission
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual int Value
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
        public virtual AuthorityPage AuthorityPage
        {
            get;
            set;
        }
    }
}
