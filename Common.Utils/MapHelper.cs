// *************************************************************
//
// 文件名(File Name)：MapHelper
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/16 14:40:14
//
// 修改记录(Revision History)：
//		R1：
//			修改作者：
//			修改日期：
//			修改描述：
//
//		R2：
//			修改作者：
//			修改日期：
//			修改描述：
//
// *************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Api.Model.VO;
using Newtonsoft.Json;

namespace Common.Utils
{
    public class MapHelper
    {
        private static readonly string LocationAppKey = ConfigurationManager.AppSettings["locationAppKey"];
        private static readonly string LocationServiceApiHost = ConfigurationManager.AppSettings["locationServiceApiHost"];
        private static readonly string LocationServiceApiUrl = ConfigurationManager.AppSettings["locationServiceApiUrl"];
        private static readonly string LocationServiceApiKey = ConfigurationManager.AppSettings["locationServiceApiKey"];
        private const double EARTH_RADIUS = 6378.137;
        private const double PI = 3.14159265358979324 * 3000.0 / 180.0;

        private static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 计算两个坐标之间的距离
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns>单位：米</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = Rad(lat1);
            double radLat2 = Rad(lat2);
            double a = radLat1 - radLat2;
            double b = Rad(lng1) - Rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS; s = Math.Round(s * 10000) / 10000 * 1000;
            return s;
        }

        ///// <summary>
        ///// GCJ02系坐标转DB09系坐标
        ///// </summary>
        ///// <param name="lat"></param>
        ///// <param name="lng"></param>
        ///// <returns></returns>
        //public static Location GCJ02ToBD09(double lat, double lng)
        //{
        //    double x = lng;
        //    double y = lat;
        //    double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * PI);
        //    double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * PI);

        //    return new Location { Latitude = z * Math.Sin(theta) + 0.006, Longitude = z * Math.Cos(theta) + 0.0065 };
        //} 

        ///// <summary>
        ///// BD09系坐标转GCJ02系坐标
        ///// </summary>
        ///// <param name="lat"></param>
        ///// <param name="lng"></param>
        ///// <returns></returns>
        //public static Location BD09ToGCJ02(double lat, double lng)
        //{
        //    double x = lng - 0.0065;
        //    double y = lat - 0.006;
        //    double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * PI);
        //    double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * PI);

        //    return new Location{ Latitude = z * Math.Sin(theta), Longitude = z * Math.Cos(theta) };
        //}

        ///// <summary>
        ///// 逆地址解析
        ///// </summary>
        ///// <param name="location"></param>
        ///// <returns></returns>
        //public static Dictionary<string,string> LocationTransfer(Location location)
        //{
        //    //计算签名
        //    //1.首先对参数进行排序：按参数名升序
        //    SortedDictionary<string, string> pairs = new SortedDictionary<string, string>
        //    {
        //        { "key", LocationAppKey },
        //        { "location", location.Latitude + "," + location.Longitude }
        //    };
        //    //2.计算签名
        //    //请求路径+”?”+请求参数+SK进行拼接，并计算拼接后字符串md5值，即为签名(sig)：
        //    StringBuilder builder = new StringBuilder();
        //    builder.Append(LocationServiceApiUrl + "?");

        //    int index = 1;
        //    foreach (KeyValuePair<string, string> item in pairs)
        //    {
        //        if (index == pairs.Count)
        //        {
        //            builder.Append(item.Key + "=" + item.Value);
        //        }
        //        else
        //        {
        //            builder.Append(item.Key + "=" + item.Value + "&");
        //        }

        //        index++;
        //    }
        //    builder.Append(LocationServiceApiKey);

        //    byte[] bte = Encoding.Default.GetBytes(builder.ToString());
        //    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        //    byte[] output = md5.ComputeHash(bte);

        //    var sign = BitConverter.ToString(output).Replace("-", "");
        //    sign = sign.ToLower();

        //    //3.生成最终请求：将计算得到的签名放到请求中,参数名即为：sig
        //    HttpClient client = new HttpClient();
        //    string url = LocationServiceApiHost + LocationServiceApiUrl + "?key={0}&location={1}&sig={2}";
        //    url = string.Format(url, LocationAppKey, location.Latitude + "," + location.Longitude, sign);
        //    HttpResponseMessage response = client.GetAsync(url).Result;
        //    response.EnsureSuccessStatusCode();

        //    string msg = response.Content.ReadAsStringAsync().Result;
        //    dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

        //    if ((int)data.status == 0)
        //    {
        //        Dictionary<string, string> dict = new Dictionary<string, string>();

        //        if (data.result.formatted_addresses == null)
        //        {
        //            dict.Add("errcode", ExceptionHelper.WXLOCATIONNOTFOUND.ToString());
        //            dict.Add("errmsg", "没有地址信息");
        //        }
        //        dict.Add("result", (string)data.result.formatted_addresses.recommend);
        //        return dict;
        //    }
        //    else
        //    {
        //        Dictionary<string, string> dict = new Dictionary<string, string>
        //            {
        //                { "errcode", ExceptionHelper.WXLOCATIONTRANSERROR.ToString() },
        //                { "errmsg", "地址解析出错。"+(string)data.message }
        //            };
        //        return dict;
        //    }
        //}
    }
}
