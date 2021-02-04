using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Common.Report
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/9/30 10:58:56
	*描  述  ：
	***********************************************************************/
    public class XmlHelperx
    {
        /// <summary>
        /// 读取XMl文件
        /// </summary>
        /// <param name="path">xml路径</param>
        public static T ReadToString<T>(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                var xmlReader = new XmlTextReader(stream);

                return (T)serializer.Deserialize(xmlReader);
            }
        }
        /// <summary>
        /// 保存到XML文件
        /// </summary>
        /// <param name="json">字符串</param>
        /// <param name="obj">xml对象</param>
        public static void WriteToFile<T>(string path)
        {
            var serializer = new XmlSerializer(typeof(T));
            File.WriteAllText(path, "");
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                serializer.Serialize(stream, typeof(T));
            }
        }
    }
}
