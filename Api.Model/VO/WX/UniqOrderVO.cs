using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO.WX
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-01-15 18:03:50
	*描  述  ：
	***********************************************************************/
    public class UniqOrderVO
    {
        public virtual string Openid
        {
            get;
            set;
        }
        public virtual string OrderNo
        {
            get;
            set;
        }
        public virtual string OrderAmount
        {
            get;
            set;
        }
    }
}
