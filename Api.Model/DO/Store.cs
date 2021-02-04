using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DO
{
    /// <summary>
    /// 仓库管理
    /// </summary>
   public class Store
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public virtual int FID { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public virtual string FCODE { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public virtual string FNAME { get; set; }

        /// <summary>
        /// 地区id
        /// </summary>
        public virtual int FAREAID { get; set; }

        /// <summary>
        /// 地区名称
        /// </summary>
        public virtual string FAREANAME { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public virtual int FENABLE { get; set; }


        /// <summary>
        /// k3编号
        /// </summary>
        public virtual string FK3CODE { get; set; }
        /// <summary>
        /// 代理商id
        /// </summary>
        public virtual string fagentid { get; set; }
        /// <summary>
        /// 代理商名称
        /// </summary>
        public virtual string fagentname { get; set; }


    }
}
