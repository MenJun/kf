using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Report.Core
{
    interface IXmlService
    {
        T ReadAsObject<T>(string path);
       
        void UpdateDocument<T>(string path, T entity);
    }
}
