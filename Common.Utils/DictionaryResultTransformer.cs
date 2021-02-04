using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/4 10:09:51
	*描  述  ：
	***********************************************************************/
    public class DictionaryResultTransformer : IResultTransformer
    {

        public IList TransformList(IList collection)
        {
            return collection;
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < aliases.Length; i++)
            {
                string alias = aliases[i];
                if (alias != null)
                {
                    if(tuple[i] == null)
                    {
                        result[alias.ToLower()] = "";
                    }
                    else if(tuple[i].GetType() == typeof(String))
                    {
                        result[alias.ToLower()] = tuple[i].ToString().Trim('\0');
                    }
                    else
                    {
                        result[alias.ToLower()] = tuple[i];
                    }
                    
                }
            }
            return result;
        }
    }
}
