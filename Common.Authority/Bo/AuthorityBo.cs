using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Bo
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/23 9:22:13
	*描  述  ：
	***********************************************************************/

    public class Rootobject
    {
        public IList<AuthorityBo> Modules { get; set; }
    }

    public class AuthorityBo
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Value { get; set; }
        public bool IsCommon { get; set; }
        public IList<AuthorityBo> Children { get; set; }
    }

}
