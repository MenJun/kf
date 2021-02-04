using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-02-10 14:34:58
	*描  述  ：
	***********************************************************************/
    public class WxShareVO
    {
        public string Code
        {
            get;
            set;
        }

        public string Uuid
        {
            get;
            set;
        }

        public string WxOpenId
        {
            get;
            set;
        }

        public int GoodsId
        {
            get;
            set;
        }

        public int ActivityId
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }
    }
}
