using Common.WxService.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common.Utils;

namespace Common.WxService
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-03-05 15:35:42
	*描  述  ：
	***********************************************************************/
    public class WxHelper
    {
        private static readonly object locker = new object();

        /// <summary>
        /// 获取请求凭据
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken(string appId, string secret)
        {
            var token = RedisHelper.StringGet("access_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                lock (locker)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        HttpClient client = new HttpClient();
                        string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";

                        url = string.Format(url, appId, secret);
                        var result = client.GetStringAsync(url).Result;
                        var rsp = JsonConvert.DeserializeObject<RspAccessToken>(result);
                        if (string.IsNullOrWhiteSpace(rsp.Errmsg))
                        {
                            //提前5分钟失效
                            DateTime expireTime = DateTime.Now.AddSeconds(rsp.Expires_in - 300);
                            TimeSpan expireTs = new TimeSpan(expireTime.Ticks);
                            TimeSpan nowTs = new TimeSpan(DateTime.Now.Ticks);

                            RedisHelper.StringSet("access_token", rsp.Access_token, expireTs.Subtract(nowTs));
                            token = rsp.Access_token;
                        }
                        else
                        {
                            throw new Exception("获取微信请求凭据失败！" + rsp.Errmsg);
                        }
                    }
                }
            }
            return token;
        }

        public static string GetQywxAccessToken(string appId, string secret)
        {
            var token = RedisHelper.StringGet("qywx_access_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                lock (locker)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        HttpClient client = new HttpClient();

                        string url = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";

                        url = string.Format(url, appId, secret);
                        var result = client.GetStringAsync(url).Result;
                        var rsp = JsonConvert.DeserializeObject<RspAccessToken>(result);
                        if (rsp.Errmsg == "ok")
                        {
                            //提前5分钟失效
                            DateTime expireTime = DateTime.Now.AddSeconds(rsp.Expires_in - 300);
                            TimeSpan expireTs = new TimeSpan(expireTime.Ticks);
                            TimeSpan nowTs = new TimeSpan(DateTime.Now.Ticks);

                            RedisHelper.StringSet("qywx_access_token", rsp.Access_token, expireTs.Subtract(nowTs));
                            token = rsp.Access_token;
                        }
                        else
                        {
                            throw new Exception("获取企业微信请求凭据失败！" + rsp.Errmsg);
                        }
                    }
                }
            }
            return token;
        }

        /// <summary>
        /// 获取签名数据
        ///</summary>
        /// <param name="strParam"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSignInfo(Dictionary<string, string> strParam, string key)
        {
            int i = 0;
            string sign = string.Empty;
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> temp in strParam)
            {
                if (temp.Value == "" || temp.Value == null || temp.Key.ToLower() == "sign")
                {
                    continue;
                }
                i++;
                sb.Append(temp.Key.Trim() + "=" + temp.Value.Trim() + "&");
            }
            sb.Append("key=" + key.Trim() + "");

            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bt = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            sign = BitConverter.ToString(bt).Replace("-", "").ToUpper();
            return sign;
        }

        /// <summary>
        /// 获取XML值
        /// </summary>
        /// <param name="strXml">XML字符串</param>
        /// <param name="strData">字段值</param>
        /// <returns></returns>
        public static string GetXmlValue(string strXml, string strData)
        {
            string xmlValue = string.Empty;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(strXml);
            var selectSingleNode = xmlDocument.DocumentElement.SelectSingleNode(strData);
            if (selectSingleNode != null)
            {
                xmlValue = selectSingleNode.InnerText;
            }
            return xmlValue;
        }

        /// <summary>
        /// 集合转换XML数据 (拼接成XML请求数据)
        /// </summary>
        /// <param name="strParam">参数</param>
        /// <returns></returns>
        public static string CreateXmlParam(Dictionary<string, string> strParam)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<xml>");
            foreach (KeyValuePair<string, string> k in strParam)
            {
                if (k.Key == "attach" || k.Key == "body" || k.Key == "sign")
                {
                    sb.Append("<" + k.Key + "><![CDATA[" + k.Value + "]]></" + k.Key + ">");
                }
                else
                {
                    sb.Append("<" + k.Key + ">" + k.Value + "</" + k.Key + ">");
                }
            }
            sb.Append("</xml>");

            return sb.ToString();
        }

        /// <summary>
        /// XML数据转换集合（XML数据拼接成字符串)
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFromXml(string xmlString)
        {
            Dictionary<string, string> sParams = new Dictionary<string, string>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlElement root = doc.DocumentElement;
            int len = root.ChildNodes.Count;
            for (int i = 0; i < len; i++)
            {
                string name = root.ChildNodes[i].Name;
                if (!sParams.ContainsKey(name))
                {
                    sParams.Add(name.Trim(), root.ChildNodes[i].InnerText.Trim());
                }
            }

            return sParams;
        }

        /// <summary>
        /// 返回通知 XML
        /// </summary>
        /// <param name="returnCode"></param>
        /// <param name="returnMsg"></param>
        /// <returns></returns>
        public static string GetReturnXml(string returnCode, string returnMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<xml>");
            sb.Append("<return_code><![CDATA[" + returnCode + "]]></return_code>");
            sb.Append("<return_msg><![CDATA[" + returnMsg + "]]></return_msg>");
            sb.Append("</xml>");
            return sb.ToString();
        }
    }
}
