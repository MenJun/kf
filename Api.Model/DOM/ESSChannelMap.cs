using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    public class ESSChannelMap: ClassMap<ESSChannelStaff>
    {
        public ESSChannelMap()
        {
            Id(x => x.FID);
            Map(x => x.FCHANNELID);
            Map(x => x.FMOBILE);

            Map(x => x.FTELE);
            Map(x => x.FQQ);
            Map(x => x.FWXOPENID);
            Map(x => x.FWECHAT);
            Map(x => x.BIRTHDAY);
            Map(x => x.GENDER);
            Map(x => x.AREA);
            Map(x => x.ISNEW);
            Map(x => x.FCREATEDATE);
            Map(x => x.FMODIFIERID);
            Map(x => x.FMODIFYDATE);
            Map(x => x.FENABLE);
            Map(x => x.SALT);
            Map(x => x.PASSWORD);

            Map(x => x.KHNAME);
            Map(x => x.KHSH);
            Map(x => x.KHZCDZ);
            Map(x => x.KHSHDZ);
            Map(x => x.KHTEL);
            Map(x => x.KHBANK);
            Map(x => x.KHBANKZH);
            Map(x => x.A3ID);
            Map(x => x.XCXOPENID);
            Table("T_ESS_CHANNELSTAFF");
        }
    }
}
