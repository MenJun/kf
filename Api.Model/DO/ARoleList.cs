using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DO
{
    public class ARoleList
    {
        public virtual Int64 XH { get; set; }
        public virtual int FID { get; set; }
        public virtual string FNAME { get; set; }
        public virtual int FSTOREID { get; set; }
        public virtual string FSTORENAME { get; set; }
    }
}
