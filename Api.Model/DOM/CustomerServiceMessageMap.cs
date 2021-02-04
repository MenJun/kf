using Api.Model.DO;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.DOM
{
    public class CustomerServiceMessageMap:ClassMap<CustomerServiceMessage>
    {
        public CustomerServiceMessageMap()
        {
            Id(x => x.Id);
            Map(x => x.MsgId);
            Map(x => x.FromUserName);
            Map(x => x.ToUserName);
            Map(x => x.CreateTime);
            Map(x => x.MsgType);
            Map(x => x.Content);
            Map(x => x.Title);
            Map(x => x.AppId);
            Map(x => x.PicUrl);
            Map(x => x.MediaId);
            Map(x => x.PagePath);
            Map(x => x.ThumbUrl);
            Map(x => x.ThumbMediaId);
            Map(x => x.Format);
            Map(x => x.Recognition);
            Map(x => x.XCXFromOpenId);
            Map(x => x.XCXToOpenId);

            Table("T_CUS_SERVER_MSG");
        }
    }
}
