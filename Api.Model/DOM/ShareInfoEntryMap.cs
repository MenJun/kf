using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-02-10 19:03:37
	*描  述  ：
	***********************************************************************/
    public class ShareInfoEntryMap:ClassMap<ShareInfoEntry>
    {
        public ShareInfoEntryMap()
        {
            Id(x => x.Id);
            Map(x=>x.Uuid);
            Map(x => x.WxOpenId);
            Map(x => x.Date);

            Table("A_SHAREINFOENTRY");
        }
    }
}
