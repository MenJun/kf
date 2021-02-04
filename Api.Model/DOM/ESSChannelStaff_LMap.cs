using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    public class ESSChannelStaff_LMap : ClassMap<ESSChannelStaff_L>
    {
        public ESSChannelStaff_LMap()
        {
            Id(x => x.FID).GeneratedBy.Assigned();
            Map(x => x.FPKID);
            Map(x => x.FLOCALEID);
            Map(x => x.FNAME);
            Map(x => x.FJOB);
            Map(x => x.FROLEID);
            Map(x => x.FREMARK);

            Table("T_ESS_CHANNELSTAFF_L");
        }
    }
}
