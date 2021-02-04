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
	*创建时间：2020-02-10 14:57:47
	*描  述  ：
	***********************************************************************/
    public class ShareInfoMap:ClassMap<ShareInfo>
    {
        public ShareInfoMap()
        {
            Id(x => x.Id);
            Map(x => x.Uuid);
            Map(x => x.WxOpenId);
            Map(x => x.GoodsId);
            Map(x => x.ActivityId);
            Map(x => x.Date);

            Table("A_SHAREINFO");
        }
    }
}
