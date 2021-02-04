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
	*创建时间：2019/10/22 10:16:27
	*描  述  ：
	***********************************************************************/
    public class RolePermissionMap:ClassMap<RolePermission>
    {
        public RolePermissionMap()
        {
            Id(x => x.Id);

            References(x => x.Role).Column("RoleId");
            References(x => x.AuthorityPage).Column("AuthorityPageId");
            References(x => x.Permission).Column("PermissionId");

            Table("A_CUS_ROLEPERMISSION");
        }
    }
}
