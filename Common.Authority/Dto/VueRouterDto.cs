using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Dto
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/23 13:32:03
	*描  述  ：
	***********************************************************************/
    public class VueRouterRootDto
    {
        public IList<VueRouterDto> Routers
        {
            get;
            set;
        }
    }

    public class VueRouterDto
    {
        public string Path
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool IsPage
        {
            get;
            set;
        }
        public string Alias
        {
            get;
            set;
        }
        public bool NotAuth
        {
            get;
            set;
        }
        public VueRouterMeta Meta
        {
            get;
            set;
        }
        public string ComponentPath
        {
            get;
            set;
        }
        public IList<VueRouterDto> Children
        {
            get;
            set;
        }

        public VueRouterDto()
        {
            Children = new List<VueRouterDto>();
        }
    }

    public class VueRouterMeta
    {
        public string Title
        {
            get;
            set;
        }
        public string Menu
        {
            get;
            set;
        }
    }
}
