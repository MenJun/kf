using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Dto
{
    public class PermissionTreeDto
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
        public IList<PermissionTreeDto> Children
        {
            get;
            set;
        }
    }
}
