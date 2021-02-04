using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.BO
{
    public class SaleOrderExportExcelBO
    {


        public string 编号
        {
            get;
            set;
        }
        public string A3客户ID
        {
            get;
            set;
        }
        public DateTime 订单日期
        {
            get;
            set;
        }
        public string 代理商
        {
            get;
            set;
        }
        public string 单据状态
        {
            get;
            set;
        }
        public string 商品编码
        {
            get;
            set;
        }
        public Decimal 折扣
        {
            get;
            set;
        }
        public Decimal 订货数量
        {
            get;
            set;
        }
        public Decimal 标准价格
        {
            get;
            set;
        }
        public Decimal 实际价格
        {
            get;
            set;
        }
        public Decimal 价格合计
        {
            get;
            set;
        }
    
        
    }
}
