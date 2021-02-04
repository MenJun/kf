// *************************************************************
//
// 文件名(File Name)：ResponseModel
//
// 功能描述(Description)：
// 
// 作者(Author)：asus
//
// 创建日期(Create Date)：2019/1/17 9:43:25
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    public class Response
    {
        public int Errcode
        {
            get;
            set;
        }

        public string Errmsg
        {
            get;
            set;
        }

        public object Result
        {
            get;
            set;
        }
        //public int zk
        //{
        //    get;
        //    set;
        //}
        //public string zc
        //{
        //    get;
        //    set;
        //}
    }
}
