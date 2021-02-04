using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    public class ChannelStaffVO
    {
        public virtual int FID { get; set; }
        public virtual int FCHANNELID { get; set; }
        [Required(ErrorMessage ="手机号不能为空")]
        public virtual string FMOBILE { get; set; }
        public virtual bool ISNEW { get; set; }
        public virtual string FTELE { get; set; }
        public virtual string FQQ { get; set; }
        public virtual string BIRTHDAY { get; set; }
        public virtual string GENDER { get; set; }
        public virtual string AREA { get; set; }
        public virtual string FWXOPENID { get; set; }
        public virtual string FWECHAT { get; set; }
        public virtual int FCREATORID { get; set; }
        public virtual DateTime FCREATEDATE { get; set; }
        public virtual int FMODIFIERID { get; set; }
        public virtual DateTime FMODIFYDATE { get; set; }
        public virtual char FENABLE { get; set; }
        public virtual string SALT { get; set; }
        public virtual string PASSWORD { get; set; }

        // 客户名称   
        public virtual string KHNAME { get; set; }

        // 客户公司税号
  
        public virtual string KHSH { get; set; }

  
        public virtual string KHZCDZ { get; set; }

        // 客户收货地址
       
        public virtual string KHSHDZ { get; set; }

        // 客户联系电话
      
        public virtual string KHTEL { get; set; }

        // 客户开户行名称
      
        public virtual string KHBANK { get; set; }

        // 客户银行账号
    
        public virtual string KHBANKZH { get; set; }

        public virtual long A3ID { get; set; }

        public virtual ChannelStaffLVO ChannelStaffLVOs { get; set; }
        public virtual string XCXOPENID { get; set; }
    }
}
