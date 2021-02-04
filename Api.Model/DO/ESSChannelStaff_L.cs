using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DO
{
    public class ESSChannelStaff_L
    {
        public virtual int FID { get; set; }
        public virtual int FPKID { get; set; }
        public virtual int FLOCALEID { get; set; }
        public virtual string FNAME { get; set; }
        public virtual string FJOB { get; set; }
        public virtual int FROLEID { get; set; }
        public virtual string FREMARK { get; set; }

    }
}
