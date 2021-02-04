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
	*创建时间：2019/10/22 9:58:08
	*描  述  ：
	***********************************************************************/
    public class AuthorityPageMap:ClassMap<AuthorityPage>
    {
        public AuthorityPageMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Enable);
            Map(x => x.NotAuth);

            References(x => x.AuthorityModule).Column("AuthorityModuleId").Cascade.None();
            HasMany(x => x.Permissions).Inverse().Cascade.AllDeleteOrphan();

            Table("A_CUS_AUTHPAGE");
        }
    }
}
