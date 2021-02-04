using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    public class PermissionTree
    {
        public string Title
        {
            get;
            set;
        }
        public string Category
        {
            get;
            set;
        }
        public int Id
        {
            get;
            set;
        }
        public bool Expand
        {
            get;
            set;
        }
        public bool Selected
        {
            get;
            set;
        }

        public bool Checked
        {
            get;
            set;
        }
        public IList<PermissionTree> Children
        {
            get;
            set;
        }
    }
}
