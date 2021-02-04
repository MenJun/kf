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
	*创建时间：2019/10/22 9:42:51
	*描  述  ：
	***********************************************************************/
    public class RoleMap : ClassMap<Role>
    {
        public RoleMap()
        {
            Id(x => x.Id).Column("FID");
            Map(x => x.Name).Column("FNAME");
            Map(x => x.StoreId).Column("FSTOREID");

            HasMany(x => x.RoleModules).Inverse().Cascade.AllDeleteOrphan();
            HasMany(x => x.RolePermissions).Inverse().Cascade.AllDeleteOrphan();

            Table("A_ROLE");
        }
    }
}
