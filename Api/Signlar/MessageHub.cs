using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Signlar
{
    [HubName("messageHub")]
    public class MessageHub : Hub
    {

    }
}