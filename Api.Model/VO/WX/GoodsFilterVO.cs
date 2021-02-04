using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO.WX
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019-12-31 16:36:59
	*描  述  ：
	***********************************************************************/
    public class GoodsFilterVO
    {
        /// <summary>
        /// 分类
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 活动商品=1 全部商品=0
        /// </summary>
        public int Category { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string Kw { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// 每页显示的数量
        /// </summary>
        public int PageSize { get; set; }
    }
}
