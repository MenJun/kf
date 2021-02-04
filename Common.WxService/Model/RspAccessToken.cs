using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.WxService.Model
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-03-05 16:54:19
	*描  述  ：
	***********************************************************************/
    public class RspAccessToken
    {
        public string Access_token
        {
            get;
            set;
        }
        public int Expires_in
        {
            get;
            set;
        }
        public int Errcode
        {
            get;
            set;
        }
        public string Errmsg
        {
            get;
            set;
        }
    }
}
