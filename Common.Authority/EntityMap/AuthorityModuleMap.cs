using Common.Authority.Entity;
using FluentNHibernate.Mapping;

namespace Common.Authority.EntityMap
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 9:56:21
	*描  述  ：
	***********************************************************************/
    public class AuthorityModuleMap : ClassMap<AuthorityModule>
    {
        public AuthorityModuleMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Icon);
            Map(x => x.Enable);

            HasMany(x => x.AuthorityPages).Inverse().Cascade.AllDeleteOrphan();

            Table("A_CUS_AUTHMODULE");
        }
    }
}
