using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    /// <summary>
    /// 仓库管理
    /// </summary>
    public class Store_Mapper : ClassMap<Store>
    {
        public Store_Mapper()
        {
            Id(x => x.FID);
            Map(x => x.FCODE);
            Map(x => x.FNAME);
            Map(x => x.FENABLE);
            Map(x => x.FAREAID);
            Map(x => x.FAREANAME);
            Map(x => x.FK3CODE);
            Map(x => x.fagentid);
            Map(x => x.fagentname);

            Table("T_STORE");
        }
    }
}
