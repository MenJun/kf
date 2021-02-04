using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    public class ARole_Mapper:ClassMap<ARole>
    {
        public ARole_Mapper()
        {
            Id(x => x.FID);
            Map(x => x.FNAME);
            Map(x => x.FSTOREID);

            Table("A_ROLE");
        }
    }
}
