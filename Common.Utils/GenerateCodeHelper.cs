using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public static class GenerateCodeHelper
    {
        private static readonly string REDIS_AREA_CODE_KEY = "area_key";
        private static readonly string REDIS_STORE_CODE_KEY = "store_key";
        private static readonly string REDIS_Agent_CODE_KEY = "agent_key";
        private static readonly string REDIS_Goods_CODE_KEY = "goods_key";
        /// <summary>
        /// 生成地区管理编码
        /// </summary>
        /// <returns></returns>
        public static string CreateAreaManagerCode()
        {
            string number = RedisHelper.StringGet(REDIS_AREA_CODE_KEY);
            if (string.IsNullOrWhiteSpace(number))
            {
                long startNum = 10001;
                RedisHelper.StringIncrementSet(REDIS_AREA_CODE_KEY, startNum);
                number = RedisHelper.StringGet(REDIS_AREA_CODE_KEY);
            }
            else
            {
                RedisHelper.StringIncrementSet(REDIS_AREA_CODE_KEY, 1);
            }
            return number;
        }

        /// <summary>
        /// 生成仓库管理编码
        /// </summary>
        /// <returns></returns>
        public static string CreateStoreManagerCode()
        {
            string number = RedisHelper.StringGet(REDIS_STORE_CODE_KEY);
            if (string.IsNullOrWhiteSpace(number))
            {
                long startNum = 10001;
                RedisHelper.StringIncrementSet(REDIS_STORE_CODE_KEY, startNum);
                number = RedisHelper.StringGet(REDIS_STORE_CODE_KEY);
            }
            else
            {
                RedisHelper.StringIncrementSet(REDIS_STORE_CODE_KEY, 1);
            }
            return number;
        }

        /// <summary>
        /// 生成代理管理编码
        /// </summary>
        /// <returns></returns>
        public static string CreateAgentManagerCode()
        {
            string number = RedisHelper.StringGet(REDIS_Agent_CODE_KEY);
            if (string.IsNullOrWhiteSpace(number))
            {
                long startNum = 10001;
                RedisHelper.StringIncrementSet(REDIS_Agent_CODE_KEY, startNum);
                number = RedisHelper.StringGet(REDIS_Agent_CODE_KEY);
            }
            else
            {
                RedisHelper.StringIncrementSet(REDIS_Agent_CODE_KEY, 1);
            }
            return number;
        }
        /// <summary>
        /// 生成商品管理编码
        /// </summary>
        /// <returns></returns>
        public static string CreateGoodsManagerCode()
        {
            string number = RedisHelper.StringGet(REDIS_Goods_CODE_KEY);
            if (string.IsNullOrWhiteSpace(number))
            {
                long startNum = 10001;
                RedisHelper.StringIncrementSet(REDIS_Goods_CODE_KEY, startNum);
                number = RedisHelper.StringGet(REDIS_Goods_CODE_KEY);
            }
            else
            {
                RedisHelper.StringIncrementSet(REDIS_Goods_CODE_KEY, 1);
            }
            return number;
        }
    }
}
