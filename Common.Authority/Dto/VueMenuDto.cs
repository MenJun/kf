using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Dto
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/24 15:01:55
	*描  述  ：
	***********************************************************************/
    public class VueMenuDto
    {
        public string Name
        {
            get;
            set;
        }
        public string Icon
        {
            get;
            set;
        }
        public IList<VueMenuPageDto> Pages
        {
            get;
            set;
        }
        public VueMenuDto()
        {
            Pages = new List<VueMenuPageDto>();
        }
    }

    public class VueMenuPageDto
    {
        public string Name
        {
            get;
            set;
        }
        public int Permission
        {
            get;
            set;
        }
    }
}
