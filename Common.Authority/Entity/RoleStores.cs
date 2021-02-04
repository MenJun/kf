using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Entity
{
    public class RoleStores
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual int RoleId
        {
            get;
            set;
        }
        public virtual int StoreId
        {
            get;
            set;
        }
    }
}
