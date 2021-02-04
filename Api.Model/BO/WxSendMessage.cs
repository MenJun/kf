using Api.Model.DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.BO
{
    public class WxSendMessage : CustomerServiceMessage
    {
        public string PICTURE { get; set; }
    }
}
