using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-04-27 16:12:04
	*描  述  ：
	***********************************************************************/
    public class CustomerServiceMessageVO
    {
        public virtual long Id { get; set; }
        public virtual long MsgId { get; set; }
        public virtual string ToUserName { get; set; }
        public virtual string FromUserName { get; set; }
        public virtual long CreateTime { get; set; }
        public virtual string MsgType { get; set; }
        public virtual string Content { get; set; }
        public virtual string Title { get; set; }
        public virtual string AppId { get; set; }
        public virtual string PicUrl { get; set; }
        public virtual string PagePath { get; set; }
        public virtual string MediaId { get; set; }
        public virtual string ThumbUrl { get; set; }
        public virtual string ThumbMediaId { get; set; }
        public virtual string Format { get; set; }
        public virtual string Recognition { get; set; }
        public virtual string XCXFromOpenId { get; set; }
        public virtual string XCXToOpenId { get; set; }

        public virtual object Image { get; set; }
    }
}
