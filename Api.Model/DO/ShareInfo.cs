using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-02-10 14:53:28
	*描  述  ：
	***********************************************************************/
    public class ShareInfo
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual string Uuid
        {
            get;
            set;
        }

        public virtual string WxOpenId
        {
            get;
            set;
        }
        public virtual int GoodsId
        {
            get;
            set;
        }
        public virtual int ActivityId
        {
            get;
            set;
        }
        public virtual DateTime Date
        {
            get;
            set;
        }
    }
}
