// *************************************************************
//
// 文件名(File Name)：TimestampHelper
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/14 14:53:23
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

namespace Common.Utils
{
    /// <summary>
    /// 时间戳与日期相互转换帮组类
    /// </summary>
    public class TimeStampHelper
    {
        /// <summary>
        /// 时间戳转日期
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime FromTimeStamp(long unixTimeStamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 8, 0, 0, 0));
            return startTime.AddSeconds(unixTimeStamp);
        }
        /// <summary>
        /// 日期转时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimeStamp(DateTime dateTime)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 8, 0, 0, 0);
            return (long)(dateTime - startTime).TotalSeconds;
        }
    }
}
