using Api.Model.DO;
using Api.Model.VO;
using Api.Model.VO.WX;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Api
{
    public class AutoMapperProfile : Profile
    {
        public static readonly string ImageHost = ConfigurationManager.AppSettings["imageHost"];
        /// <summary>
        /// 
        /// </summary>
        public AutoMapperProfile()
        {
            ////销售退货主表
            //CreateMap<ESSReturnReq, ReturnReqVO>();
            //CreateMap<ReturnReqVO, ESSReturnReq>();
         

            //CreateMap<WxOrderVO, POrders>()
            //    .ForMember(x => x.OrderCommodities, opt => opt.MapFrom(p => p.OrderCommodities))
            //    .ForMember(x => x.POrdersEntryVOs, opt => opt.MapFrom(p => p.POrdersEntryVOs));
        }
    }
}