using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    public class ESSChannelStaffAvatarMap : ClassMap<ESSChannelStaffAvatar>
    {
        public ESSChannelStaffAvatarMap()
        {
            Id(x => x.Id);
            Map(x => x.UseWxAvatar);
            Map(x => x.Picture);
            Map(x => x.StaffId);

            Table("T_ESS_CHANNELSTAFF_AVATAR");
        }
    }
}
