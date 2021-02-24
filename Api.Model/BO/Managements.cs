using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.BO
{
    public class Managements
    {
        //   "khname": "mmm",
        //"xcxopenid": "oV3Tj5F0Ed8HTrbZnFGDTNX-zxWA",
        //"fmobile": "18598988531",
        //"fwxopenid": "ovBlpvxbaECET8rTTOxySiJdM9A4",
        //"fid": 120567,
        //"zfid": 120585
        public string khname { get; set; }
        public string xcxopenid { get; set; }
        public string fmobile { get; set; }
        public long fid { get; set; }
        public long zfid { get; set; }
        public string fwxopenid { get; set; }

        List<sonXMText> sonxmtext = new List<sonXMText>();
        public List<sonXMText> sonXMText
        {
            get { return sonxmtext; }
            set { sonxmtext = value; }
        }
    }
}
