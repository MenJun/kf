using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    public class ChannelStaffLVO
    {
        public virtual int FID { get; set; }
        public virtual int FPKID { get; set; }
        public virtual int FLOCALEID { get; set; }
        //[Required(ErrorMessage ="职员名称不能为空")]
        public virtual string FNAME { get; set; }
        public virtual string FJOB { get; set; }
        public virtual int FROLEID { get; set; }
        public virtual string FREMARK { get; set; }
    }
}
