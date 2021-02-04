using Common.Authority.Entity;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.EntityMap
{
    public class RoleStoresMap:ClassMap<RoleStores>
    {
        public RoleStoresMap()
        {
            Id(x => x.Id);
            Map(x => x.RoleId);
            Map(x => x.StoreId);

            Table("A_CUS_ROLESTORES");
        }
    }
}
