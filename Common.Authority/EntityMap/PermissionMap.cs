using Common.Authority.Entity;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.EntityMap
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:47:58
	*描  述  ：
	***********************************************************************/
    public class PermissionMap:ClassMap<Permission>
    {
        public PermissionMap()
        {
            Id(x => x.Id);
            Map(x => x.Value);
            Map(x => x.Name);
            Map(x => x.Enable);

            References(x=>x.AuthorityPage).Column("AuthorityPageId").Cascade.None();

            Table("A_CUS_PERMISSION");
        }
    }
}
