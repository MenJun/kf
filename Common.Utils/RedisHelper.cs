// *************************************************************
//
// 文件名(File Name)：RedisHelper
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 9:37:09
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
using System.Configuration;
using StackExchange.Redis;

namespace Common.Utils
{
    public static class RedisHelper
    {

        private static readonly string redisHost = ConfigurationManager.AppSettings["redisHost"];
        private static readonly string redisPort = ConfigurationManager.AppSettings["redisPort"];
        private static readonly string redisPwd = ConfigurationManager.AppSettings["redisPwd"];
        private static readonly string dbNumber = ConfigurationManager.AppSettings["redisDbNumber"];

        private static readonly object locker = new object();
        private static ConnectionMultiplexer connectionMultiplexer;
        private static IDatabase database;

        private static IDatabase GetDatabase()
        {
            if(database == null)
            {
                lock (locker)
                {
                    if(connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        string connStr = "{0}:{1},allowAdmin = true,password = {2}";
                        connStr = string.Format(connStr, redisHost, redisPort, redisPwd);
                        connectionMultiplexer = ConnectionMultiplexer.Connect(connStr);
                        database = connectionMultiplexer.GetDatabase(int.Parse(dbNumber));
                    }
                }
            }
            return database;         
        }

        /// <summary>
        /// 保存自增字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static long StringIncrementSet(string redisKey, long redisValue)
        {
            return GetDatabase().StringIncrement(redisKey, redisValue);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static bool StringSet(string redisKey, string redisValue, TimeSpan? expiry)
        {
            return GetDatabase().StringSet(redisKey, redisValue, expiry);
        }

        /// <summary>
        /// 保存一个对象，该对象会被序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="exipry"></param>
        /// <returns></returns>
        public static bool StringSet<T>(string redisKey, T redisValue, TimeSpan? exipry)
        {
            string rValue = Newtonsoft.Json.JsonConvert.SerializeObject(redisValue);
            return GetDatabase().StringSet(redisKey, rValue, exipry);
        }

        /// <summary>
        /// 删除指定字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public static bool KeyDelete(string redisKey)
        {
            return GetDatabase().KeyDelete(redisKey);
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public static string StringGet(string redisKey)
        {
            return GetDatabase().StringGet(redisKey);
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public static T StringGet<T>(string redisKey)
        {
            var objct = GetDatabase().StringGet(redisKey);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objct);
        }
    }
}
