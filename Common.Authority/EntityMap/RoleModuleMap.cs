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
	*创建时间：2019/10/22 10:10:52
	*描  述  ：
	***********************************************************************/
    public class RoleModuleMap : ClassMap<RoleModule> 
    {
        public RoleModuleMap()
        {
            Id(x => x.Id);

            References(x => x.AuthorityModule).Column("AuthorityModuleId").Cascade.None();
            References(x => x.Role).Column("RoleId").Cascade.None();

            Table("A_CUS_ROLEMODULE");
        }
    }
}
