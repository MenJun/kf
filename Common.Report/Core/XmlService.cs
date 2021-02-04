using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Report.Core
{
    public class XmlService : IXmlService
    {
        /// <summary>
        /// 读取xml文件内容
        /// </summary>
        /// <typeparam name="T">序列化目标对象</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public T ReadAsObject<T>(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                var xmlReader = new XmlTextReader(stream);

                return (T)serializer.Deserialize(xmlReader);
            }
        }

        /// <summary>
        /// 更新xml文档
        /// </summary>
        /// <typeparam name="T">反序列化目标对象</typeparam>
        /// <param name="path">文件路径</param>
        public void UpdateDocument<T>(string path, T entity)
        {
            var serializer = new XmlSerializer(typeof(T));
            File.WriteAllText(path, "");
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                serializer.Serialize(stream, entity);
            }
        }
    }
}
