using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-02-11 18:33:39
	*描  述  ：
	***********************************************************************/
    public class ESSChannelStaffAvatar
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual bool UseWxAvatar
        {
            get;
            set;
        }
        public virtual string Picture
        {
            get;
            set;
        }
        public virtual int StaffId
        {
            get;
            set;
        }
    }
}
