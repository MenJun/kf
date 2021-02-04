using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authority.Dto
{
    public class RoleDto
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual string Name
        {
            get;
            set;
        }
    }

    public class AuthorityModuleDto
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual string Name
        {
            get;
            set;
        }
        public virtual bool Checked
        {
            get;
            set;
        }
    }

    public class RoleModuleDto
    {
        public virtual RoleDto RoleDto
        {
            get;
            set;
        }
        public virtual IList<AuthorityModuleDto> AuthorityModuleDtos
        {
            get;
            set;
        }
    }
}
