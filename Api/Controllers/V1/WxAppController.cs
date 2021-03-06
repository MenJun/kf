﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using Api.Dao.V1;
using Api.Model.DO;
using Api.Model.VO;
using Api.Model.VO.WX;
using Api.Services.V1;
using Api.Signlar;
using Common.Filter;
using Microsoft.AspNet.SignalR;
using Microsoft.Web.Http;
using Newtonsoft.Json.Linq;
using Unity.Attributes;

namespace Api.Controllers.V1
{
    /// <summary>
    /// 小程序接口
    /// </summary>
    [JwtAuthorizationFilter]
    [ApiVersion("1.0")]
    [RoutePrefix("api/wxapp")]
    public class WxAppController : ApiController
    {
        [Dependency]
        public WxappService WxappService
        {
            get;
            set;
        }
        [Dependency]
        public WxappDao WxappDao
        {
            get;
            set;
        }



        /// <summary>
        /// 查询分类下商品
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("goods")]
        [Transaction]
        [HttpGet]
        public Response QueryGoodsByType([FromUri]GoodsFilterVO filter)
        {
            return WxappService.QueryGoodsByType(filter);
        }

        /// <summary>
        /// 查询用户待付款、待发货、待收货的订单数量
        /// </summary>
        /// <returns></returns>
        [Route("{uid}/order/count")]
        [HttpGet]
        [Transaction]
        public Response QueryOrderCountStatusNum([FromUri]int uid)
        {
            return WxappService.QueryOrderCountStatusNum(uid);
        }

        /// <summary>
        /// 查询用户订单
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="status">订单状态</param>
        /// <param name="kw">搜索关键字</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <returns></returns>
        [Route("{uid}/orders")]
        [HttpGet]
        [Transaction]
        public Response QueryOrders([FromUri]int uid, [FromUri]string status, [FromUri]string kw, [FromUri]int page, [FromUri]int pageSize)
        {
            return WxappService.QueryOrders(uid, status, kw, page, pageSize);
        }

        /// <summary>
        /// 查询用户分享信息
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Route("{wxopenid}/shares")]
        [HttpGet]
        [Transaction]
        public Response QueryShares([FromUri]string wxopenid, [FromUri]int page, [FromUri]int pageSize)
        {
            return WxappService.QueryShares(wxopenid, page, pageSize);
        }

        /// <summary>
        /// 查询指定商品（包含多规格）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("goods/{id}")]
        [Transaction]
        [ModelValidation]
        [HttpGet]
        public Response QueryGoodsById([FromUri]int id)
        {
            return WxappService.QueryGoodsById(id);
        }

        /// <summary>
        /// 上传定制照片
        /// </summary>
        [HttpPost]
        [Route("upload/cusphoto")]
        public Response UploadCusPhoto()
        {
            return WxappService.UploadCusPhoto();
        }

        /// <summary>
        /// 上传客服图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upload/service")]
        public Response UploadServiceImg()
        {
            return WxappService.UploadServiceImg();
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [HttpPost]
        [Transaction]
        [Route("upload/avatar")]
        public Response UploadAvatar([FromUri]int id)
        {
            return WxappService.UploadAvatar(id);
        }

        /// <summary>
        /// 使用微信头像
        /// </summary>
        /// <param name="avatar">用户id</param>
        /// <returns></returns>
        [HttpPost]
        [Transaction]
        [Route("wxavatar")]
        public Response UseWxAvatar([FromBody]ESSChannelStaffAvatar avatar)
        {
            return WxappService.UseWxAvatar(avatar);
        }

        [HttpGet]
        [Transaction]
        [Route("{id}/avatar")]
        public Response QueryUseAvatar([FromUri]int id)
        {
            return WxappService.QueryUseAvatar(id);
        }

        /// <summary>
        /// 查询所有已发布的活动
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("activity/allpublish")]
        public Response QueryAllPublishActivity()
        {
            return WxappService.QueryAllPublishActivity();
        }





        /// <summary>
        /// 查询用户购物车
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("{uid}/cart")]
        public Response QueryUserCart([FromUri]int uid)
        {
            return WxappService.QueryUserCart(uid);
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("user/{uid}")]
        public Response QueryUserInfo([FromUri]int uid)
        {
            return WxappService.QueryUserInfo(uid);
        }

        /// <summary>
        /// 更新用户性别
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [Route("user/{uid}/gender")]
        public Response UpdateUserGender([FromUri]int uid, JObject jObject)
        {
            return WxappService.UpdateUserGender(uid, jObject);
        }

        /// <summary>
        /// 更新用户生日
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [AllowAnonymous]
        [Route("user/{uid}/birthday")]
        public Response UpdateUserBirthday([FromUri]int uid, JObject jObject)
        {
            return WxappService.UpdateUserBirthday(uid, jObject);
        }

        /// <summary>
        /// 更新用户所在地区
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [Route("user/{uid}/area")]
        public Response UpdateUserArea([FromUri]int uid, JObject jObject)
        {
            return WxappService.UpdateUserArea(uid, jObject);
        }

        /// <summary>
        /// 更新用户名
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [Route("user/{uid}/name")]
        public Response UpdateUserName([FromUri]int uid, JObject jObject)
        {
            return WxappService.UpdateUserName(uid, jObject);
        }

        /// <summary>
        /// 更新用户微信号
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Transaction]
        [Route("user/{uid}/wechat")]
        public Response UpdateUserWechat([FromUri]int uid, JObject jObject)
        {
            return WxappService.UpdateUserWechat(uid, jObject);
        }

        /// <summary>
        /// 获取OpenId
        /// </summary>
        /// <param name="code">调用wx.login获取到的code</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getopenid")]
        public Response GetOpenId([FromUri]string code)
        {
            return WxappService.GetOpenId(code);
        }

        /// <summary>
        /// 统一下单
        /// </summary>
        /// <param name="uniqOrder">获取到的OpenId</param>
        /// <returns></returns>
        [HttpPost]
        [Route("unifiedorder")]
        public Response UnifiedOrder([FromBody]UniqOrderVO uniqOrder)
        {
            return WxappService.UnifiedOrder(uniqOrder);
        }


        /// <summary>
        /// 查询返利
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [Route("rebate")]
        [HttpGet]
        public Response QueryUserRebate([FromUri]string phone)
        {
            //return A3Service.QueryUserRebate(phone);
            return WxappService.CalculationRebate(phone);
        }

        /// <summary>
        /// 用户分享商品
        /// </summary>
        /// <param name="wxShare"></param>
        /// <returns></returns>
        [Route("share/add")]
        [Transaction]
        [HttpPost]
        public Response AddShareInfo([FromBody]WxShareVO wxShare)
        {
            return WxappService.AddShareInfo(wxShare);
        }

        /// <summary>
        /// 用户分享浏览记录
        /// </summary>
        /// <param name="wxShare"></param>
        /// <returns></returns>
        [Route("shareentry/add")]
        [Transaction]
        [HttpPost]
        public Response AddShareInfoEntry([FromBody]WxShareVO wxShare)
        {
            return WxappService.AddShareInfoEntry(wxShare);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单Id</param>
        /// <returns></returns>
        [Route("order/{id}/cancel")]
        [Transaction]
        [HttpPut]
        public Response CancelOrder([FromUri]int id)
        {
            return WxappService.CancelOrder(id);
        }



        /// <summary>
        /// 订阅快递100物流信息
        /// </summary>
        [Route("kd100/{orderNum}/sub")]
        [Transaction]
        [HttpGet]
        public Response Kd100Sub([FromUri]string orderNum)
        {
            return WxappService.Kd100Sub(orderNum);
        }



        /// <summary>
        /// 创建小程序码
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [Route("create/xcxcode")]
        [Transaction]
        [HttpGet]
        public Response GenerateWxxcxCode([FromUri]string uuid)
        {
            return WxappService.CreateXcxCode(uuid);
        }

        /// <summary>
        /// 查询分享商品
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [Route("query/sharegoods")]
        [Transaction]
        [HttpGet]
        public Response QueryShareGoods([FromUri]string uuid)
        {
            return WxappService.QueryShareGoods(uuid);
        }

        /// <summary>
        /// 小程序消息推送配置验证
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <returns></returns>
        [Route("urlverify")]
        [HttpGet]
        [AllowAnonymous]
        [ApiVersionNeutral]
        public HttpResponseMessage VerifyMsgPullUrl([FromUri]string signature, [FromUri]string timestamp, [FromUri]string nonce, [FromUri]string echostr)
        {
            var result = WxappService.VerifyMsgPullUrl(signature, timestamp, nonce, echostr);
            // ! 重要，必需返回HttpResopnseMessage类型
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent(
                result,
                System.Text.Encoding.UTF8,
                "text/plain"
            )
            };
            return response;
        }

        [Route("urlverify")]
        [HttpPost]
        [Transaction]
        [AllowAnonymous]
        [ApiVersionNeutral]
        public async Task<string> ReceiveAndSendCustomerMsg([FromBody]CustomerServiceMessage serviceMessage)
        {
            var result = await WxappService.ReceiveCustomerMsg(serviceMessage);
            if (result != null)
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                hub.Clients.All.notify(result);
            }
            return "success";
        }

        /// <summary>
        /// 企业微信消息推送配置验证
        /// </summary>
        /// <param name="msg_signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <returns></returns>
        [Route("qy/urlverify")]
        [HttpGet]
        [AllowAnonymous]
        [ApiVersionNeutral]
        public HttpResponseMessage VerifyQyMsgPullUrl([FromUri]string msg_signature, [FromUri]string timestamp, [FromUri]string nonce, [FromUri]string echostr)
        {
            var result = WxappService.VerifyQyMsgPullUrl(msg_signature, timestamp, nonce, echostr);
            // ! 重要，必需返回HttpResopnseMessage类型
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent(
                result,
                System.Text.Encoding.UTF8,
                "text/plain"
            )
            };
            return response;
        }

        [Route("customers")]
        [HttpGet]
        [Transaction]
        public async Task<Response> QueryCustomers()
        {
            var result = await WxappService.QueryCustomers();

            return result;
        }




        /// <summary>
        /// 查询客户聊天记录
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Route("customerMsg")]
        [HttpGet]
        [Transaction]
        public async Task<IList<CustomerServiceMessageVO>> QueryCustomerMsg([FromUri]string wxopenid, [FromUri]int page, [FromUri]int limit)
        {
            var result = await WxappService.QueryCustomerMsg(wxopenid, page, limit);

            return result;
        }

        [Route("customer/lastmsg")]
        [HttpGet]
        [Transaction]
        public async Task<CustomerServiceMessageVO> QueryCustomerLastMsg([FromUri]string wxopenid)
        {
            var result = await WxappService.QueryCustomerLastMsg(wxopenid);

            return result;
        }

        [Route("staff/{id}/update")]
        [HttpPost]
        [Transaction]
        public async Task<Response> UpdateCustomerServiceStaff([FromUri]int id, [FromBody]WxPayloadVO wxPayload)
        {
            var result = await WxappService.UpdateCustomerServiceStaff(id, wxPayload);

            return result;
        }

        [Route("sendMsg")]
        [HttpPost]
        [Transaction]
        public async Task<string> SendServiceMsg([FromBody]CustomerServiceMessage serviceMessage)
        {
            var message = await WxappService.SendCustomerMsg(serviceMessage);

            var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();

            hub.Clients.All.notify(message);

            return "1";
        }

        /// <summary>
        /// 企业微信创建群聊
        /// </summary>
        /// <returns></returns>
        [Route("qywx/chat/create")]
        [HttpGet]
        public Response CreateQyexChat([FromUri]string message)
        {
            return WxappService.QywxCreateChatAndSendMsg(message);
        }


        //[HttpGet]
        //[Route("accesstoken")]
        //public Response GetAccessToken()
        //{
        //    return WxappService.GetAccessToken();
        //}


        public string jjj()
        {
            return "以下新加";
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [HttpPost]
        [Transaction]
        [Route("upload/avatar")]
        public Response ZXKH_UploadAvatar([FromUri]int id)
        {
            return WxappService.ZXKH_UploadAvatar(id);
        }


        /// <summary>
        /// 使用微信头像
        /// </summary>
        /// <param name="avatar">用户id</param>
        /// <returns></returns>
        [HttpPost]
        [Transaction]
        [Route("wxavatar")]
        public Response ZXKH_UseWxAvatar([FromBody]ESSChannelStaffAvatar avatar)
        {
            return WxappService.ZXKH_UseWxAvatar(avatar);
        }

        [HttpGet]
        [Transaction]
        [Route("{id}/avatar")]
        public Response ZXKH_QueryUseAvatar([FromUri]int id)
        {
            return WxappService.ZXKH_QueryUseAvatar(id);
        }


        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [Transaction]
        [Route("zxkh_user/{uid}")]
        public Response ZXKH_QueryUserInfo([FromUri]int uid)
        {
            return WxappService.ZXKH_QueryUserInfo(uid);
        }

        /// <summary>
        /// 获取OpenId
        /// </summary>
        /// <param name="code">调用wx.login获取到的code</param>
        /// <returns></returns>
        [HttpGet]
        [Route("zxkh_getopenid")]
        public Response ZXKH_GetOpenId([FromUri]string code)
        {
            return WxappService.ZXKH_GetOpenId(code);
        }

        /// <summary>
        /// 微信小程序-所有的用户消息，通过手机号查询客服客户关联
        /// </summary>
        /// <param name="fmobile">用户的手机号</param>
        /// <returns></returns>
        [Route("zxkh_customers")]
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        public async Task<Response> ZXKH_QueryCustomers(string fmobile)
        {
            var result = await WxappService.ZXKH_QueryCustomers(fmobile);
            return result;
        }

        /// <summary>
        /// 客服下的客户消息，通过客服手机号码查询
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>s
        [Route("zxkh_mycustomers")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_QueryMyCustomers(string fmobile)
        {
            var results = await WxappService.ZXKH_QueryMyCustomers(fmobile);

            //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //var data = new
            //{
            //    name = 1,
            //    age = 2
            //};
            //hub.Clients.All(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            //hub.Clients.All("1111");

            return results;
        }
        /// <summary>
        /// 客户下的客服消息，通过客服手机号码查询
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>
        [Route("zxkh_mystaff")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_QueryMyStaff(string fmobile)
        {
            var results = await WxappService.ZXKH_QueryMyStaff(fmobile);

            //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //var data = new
            //{
            //    name = 1,
            //    age = 2
            //};
            //hub.Clients.All(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            //hub.Clients.All("1111");

            return results;
        }
        /// <summary>
        /// 微信小程序-我的所有消息，通过手机号码查询
        /// </summary>
        /// <param name="fmobile">用户的手机号</param>
        /// <returns></returns>
        [Route("zxkh_mymessage")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<Response> ZXKH_QueryMyMessage(string fmobile)
        {
            var results = await WxappService.ZXKH_QueryMyMessage(fmobile);

            //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //var data = new
            //{
            //    name = 1,
            //    age = 2
            //};
            //hub.Clients.All(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            //hub.Clients.All("1111");

            return results;
        }


        /// <summary>
        /// 微信小程序-好友列表，通过手机号查询
        /// </summary>
        /// <param name="fmobile">用户手机号</param>
        /// <returns></returns>
        [Route("zxkh_friends")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_Friends(string fmobile)
        {
            var result = await WxappService.ZXKH_Friends(fmobile);

            return result;
        }
        /// <summary>
        /// 微信小程序-用户表，所有的用户
        /// </summary>
        /// <returns></returns>
        [Route("zxkh_user")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_User()
        {
            var result = await WxappService.ZXKH_User();

            return result;
        }
        

        /// <summary>
        /// 微信小程序-查看好友申请
        /// </summary>
        /// <param name="fmobile">用户手机号码</param>
        /// <returns></returns>
        [Route("zxkh_queryapply")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<Response> ZXKH_QueryApply(string fmobile)
        {
            var result = await WxappService.ZXKH_QueryApply(fmobile);

            return result;
        }

        /// <summary>
        /// 微信小程序-通过手机号查询用户的所有群组
        /// </summary>
        /// <param name="fmobile">用户的手机号</param>
        /// <returns></returns>
        [Route("zxkh_grouplist")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_GroupList(string fmobile)
        {
            var result = await WxappService.ZXKH_GroupList(fmobile);

            return result;
        }
        /// <summary>
        /// 微信小程序-所有的群组列表
        /// </summary>
        /// <returns></returns>
        [Route("zxkh_allgrouplist")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_ALLGroupList()
        {
            var result = await WxappService.ZXKH_ALLGroupList();

            return result;
        }
        


        /// <summary>
        /// 微信小程序-通过手机号码查询此人
        /// </summary>
        /// <param name="fmobile">用户输入的手机号</param>
        /// <returns></returns>
        [Route("zxkh_queryuser")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_QueryUser([FromUri]string fmobile)
        {
            var results = await WxappService.ZXKH_QueryUser(fmobile);

            return results;
        }
        /// <summary>
        /// 微信小程序-查找好友关系
        /// </summary>
        /// <param name="user">用户fid</param>
        /// <param name="userfriend">用户查找的fid</param>
        /// <returns></returns>
        [Route("zxkh_queryfriend")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<Response> ZXKH_QueryFriend([FromUri]long user, long userfriend)
        {
            var results = await WxappService.ZXKH_QueryFriend(user, userfriend);

            return results;
        }
        /// <summary>
        /// 微信小程序-发送好友申请
        /// </summary>
        /// <param name="user">用户fid</param>
        /// <param name="userfriend">用户查找的fid</param>
        /// <returns></returns>
        [Route("zxkh_sendapply")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_SendApply([FromUri]long user, [FromUri]long userfriend)
        {
            WxappService.ZXKH_SendApply(user, userfriend);
        }

        /// <summary>
        /// 微信小程序-用户同意好友申请
        /// </summary>
        /// <param name="user">用户的fid</param>
        /// <param name="userfriend">申请好友关系的用户fid</param>
        /// <returns></returns>
        [Route("zxkh_friendadd")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async void ZXKH_Friendadd([FromUri]long user, long userfriend)
        {
            WxappService.ZXKH_Friendadd(user, userfriend);
        }

        /// <summary>
        /// 微信小程序-创建群组
        /// </summary>
        /// <param name="array">用户的fid，数组</param>
        /// <returns></returns>
        [Route("zxkh_groupcreat")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_GroupCreat([FromUri]string array)
        {
            WxappService.ZXKH_GroupCreat(array);
        }

        /// <summary>
        /// 微信小程序-群组踢人或自己退出群聊
        /// </summary>
        /// <param name="array">用户的fid，数组</param>
        /// <param name="groupno">群编号</param>
        [Route("zxkh_groupremove")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_GroupRemove([FromUri]string array, string groupno)
        {
            WxappService.ZXKH_GroupRemove(array, groupno);
        }
        /// <summary>
        /// 微信小程序-群组邀请用户
        /// </summary>
        /// <param name="array">被邀请的人员，数组</param>
        /// <param name="groupno">群编号</param>
        [Route("zxkh_groupadd")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_GroupAdd([FromUri]string array, string groupno)
        {
            WxappService.ZXKH_GroupAdd(array, groupno);
        }

        /// <summary>
        /// 微信小程序-群创始人退出解散群，群作废
        /// </summary>
        /// <param name="groupno">群编号</param>
        /// <returns></returns>
        [Route("zxkh_groupvoid")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_GroupVoid(string groupno)
        {
            WxappService.ZXKH_GroupVoid(groupno);
        }

        /// <summary>
        /// 微信小程序-群管理头像区
        /// </summary>
        /// <param name="groupno">群编号</param>
        /// <returns></returns>
        [Route("zxkh_groupmanagement")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<Response> ZXKH_GroupManagement(string groupno)
        {
            var results = await WxappService.ZXKH_GroupManagement(groupno);

            return results;
        }

        /// <summary>
        /// 微信小程序-群资料管理-目前只有群名称名称
        /// </summary>
        /// <param name="groupno">群编号</param>
        /// <returns></returns>
        [Route("zxkh_groupinformation")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<Response> ZXKH_GroupInformation(string groupno)
        {
            var results = await WxappService.ZXKH_GroupInformation(groupno);

            return results;
        }

        /// <summary>
        /// 微信小程序-群名称编辑
        /// </summary>
        /// <param name="groupname">群名称</param>
        /// <param name="groupno">群编号</param>
        /// <returns></returns>
        [Route("zxkh_editgroupname")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public async Task<bool> ZXKH_EditGroupName(string groupname, string groupno)
        {
            var results = await WxappService.ZXKH_EditGroupName(groupname, groupno);

            return results;
        }

        /// <summary>
        /// 微信小程序-判断用户是否在此群里，目前用于实现群的实时通讯
        /// </summary>
        /// <param name="useropenid">用户的openid</param>
        /// <param name="groupno">群编号</param>
        /// <returns></returns>
        [Route("zxkh_boolgroupuser")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public bool ZXKH_BoolGroupUser(string useropenid, string groupno)
        {
            dynamic result = WxappService.ZXKH_BoolGroupUse(useropenid, groupno);
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 用户绑定connectionid  测试一
        /// </summary>
        /// <returns></returns>
        [Route("userwebsocket")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public Response ZXKH_test(string userid, string connectionid)
        {
            var result = WxappService.ZXKH_UserWebSocket(userid, connectionid);

            return result;
        }

        /// <summary>
        /// 用户发送 测试二
        /// </summary>
        /// <returns></returns>
        [Route("sendwebsocket")]
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        public void ZXKH_SendMessage(string userid)
        {

            var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            var data = new
            {
                name = 1,
                age = 22222222
            };
            //hub.Clients.All(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            //hub.Clients.All.notify(data);
            dynamic result = WxappService.ZXKH_SendWebSocket(userid);
            var user = (string)result["ConnectionID"];
            hub.Clients.Client(user).notify(data);
        }

        [Route("zxkh_khcustomer")]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_QueryKhCustomer(string fmobile)
        {
            var results = await WxappService.ZXKH_QueryKhCustomer(fmobile);

            return results;
        }


        /// <summary>
        /// 查询客户聊天记录
        /// </summary>
        /// <param name="wxopenid">好友小程序openid</param>
        /// <param name="page">页</param>
        /// <param name="openid">用户小程序openid</param>
        /// <param name="limit">一页多少</param>
        /// <returns></returns>
        [Route("zxkh_customerMsg")]
        [AllowAnonymous]
        [HttpGet]
        [Transaction]
        public async Task<IList<CustomerServiceMessageVO>> ZXKH_QueryCustomerMsg([FromUri]string wxopenid, [FromUri]string openid, [FromUri]int page, [FromUri]int limit)
        {
            var result = await WxappService.ZXKH_QueryCustomerMsg(wxopenid, openid, page, limit);

            return result;
        }
        /// <summary>
        /// 微信小程序-设置置顶消息
        /// </summary>
        /// <param name="xcxopenid">设置人的小程序openid</param>
        /// <param name="xcxopenid_top">被设置消息的openid</param>
        [Route("zxkh_message_top")]
        [AllowAnonymous]
        [HttpGet]
        [Transaction]
        public int ZXKH_Message_top(string xcxopenid, string xcxopenid_top)
        {
            return WxappService.ZXKH_Message_top(xcxopenid, xcxopenid_top);
        }
        /// <summary>
        /// 微信小程序-取消置顶消息
        /// </summary>
        /// <param name="xcxopenid">设置人的小程序openid</param>
        /// <param name="xcxopenid_top">被设置消息的openid</param>
        [Route("zxkh_message_canceltop")]
        [AllowAnonymous]
        [HttpGet]
        [Transaction]
        public void ZXKH_Message_canceltop(string xcxopenid, string xcxopenid_top)
        {
            WxappService.ZXKH_Message_canceltop(xcxopenid, xcxopenid_top);
        }

        /// <summary>
        /// 微信小程序-查询当前群聊聊天记录
        /// </summary>
        /// <param name="wxopenid">群编号</param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Route("zxkh_groupMsg")]
        [AllowAnonymous]
        [HttpGet]
        [Transaction]
        public async Task<Response> ZXKH_QueryGroupMsg([FromUri]string wxopenid, [FromUri]int page, [FromUri]int limit)
        {
            var result = await WxappService.ZXKH_QueryGroupMsg(wxopenid, page, limit);

            return result;
        }

        /// <summary>
        /// 微信小程序-查询用户最后一条消息，暂时无用
        /// </summary>
        /// <param name="wxopenid">用户小程序openid</param>
        /// <returns></returns>
        [Route("zxkh_customer/lastmsg")]
        [HttpGet]
        [Transaction]
        public async Task<CustomerServiceMessageVO> ZXKH_QueryCustomerLastMsg([FromUri]string wxopenid)
        {
            var result = await WxappService.ZXKH_QueryCustomerLastMsg(wxopenid);

            return result;
        }

        /// <summary>
        /// 微信小程序-微信小程序用户授权获取头像后进行修改
        /// </summary>
        /// <param name="id">用户fid</param>
        /// <param name="wxPayload">用户新的名称和头像</param>
        /// <returns></returns>
        [Route("zxkh_staff/{id}/update")]
        [HttpPost]
        [Transaction]
        public async Task<Response> ZXKH_UpdateCustomerServiceStaff([FromUri]int id, [FromBody]WxPayloadVO wxPayload)
        {
            var result = await WxappService.ZXKH_UpdateCustomerServiceStaff(id, wxPayload);
            
            return result;
        }

        [ApiVersionNeutral]
        [AllowAnonymous]
        [HttpGet]
        [Route("ProcessRequest")]
        public HttpResponseMessage ZXKH_CheckSignature(string signature, string timestamp, string nonce, string echostr)
        {
            //throw new Exception(string.Concat(signature, ";", timestamp, ";", nonce, ";", echostr));
            string token = "QDG6eK";
            string[] ArrTmp = { token, timestamp, nonce };
            //字典排序
            Array.Sort(ArrTmp);
            //拼接
            string tmpStr = string.Join("", ArrTmp);
            //sha1验证
            tmpStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(echostr, Encoding.UTF8, "text/plan")
                };
            }
            else
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent("error", Encoding.UTF8, "text/plan")
                };
            }
        }

        [ApiVersionNeutral]
        [AllowAnonymous]
        [HttpPost]
        [Route("ProcessRequest")]
        public void ReceiveMessage()
        {
            bool bl = false;
            //string ToUserName, string FromUserName, string MsgType, int CreateTime, string Content, int MsgId
            Stream requestStream = HttpContext.Current.Request.InputStream;
            byte[] requestByte = new byte[requestStream.Length];
            requestStream.Read(requestByte, 0, (int)requestStream.Length);
            string requestStr = Encoding.UTF8.GetString(requestByte);
            CustomerServiceMessage WxXmlModels = new CustomerServiceMessage();
            if (!string.IsNullOrEmpty(requestStr))
            {
                //封装请求类
                XmlDocument requestDocXml = new XmlDocument();
                requestDocXml.LoadXml(requestStr);
                XmlElement rootElement = requestDocXml.DocumentElement;

                ////用户发送的含有关键字“小程序”返回小程序链接
                //if (rootElement.SelectSingleNode("Content").InnerText.Contains("小程序"))
                //{
                //    var XML = WxappService.ZXKH_ReText(rootElement.SelectSingleNode("FromUserName").InnerText, rootElement.SelectSingleNode("ToUserName").InnerText, "<a data-miniprogram-appid='wx05551978a9c3ba4b' data-miniprogram-path='pages/login/main' href='http://www.qq.com'>惜时小程序</a>");
                //    HttpContext.Current.Response.Write(XML);
                //    HttpContext.Current.Response.End();
                //    bl = true;
                //}
                //点击菜单
                if (rootElement.SelectSingleNode("MsgType").InnerText == "event" && rootElement.SelectSingleNode("Event").InnerText == "CLICK" && rootElement.SelectSingleNode("EventKey").InnerText == "V1001_GOOD")
                {
                    var XML = WxappService.ZXKH_ReText(rootElement.SelectSingleNode("FromUserName").InnerText, rootElement.SelectSingleNode("ToUserName").InnerText, "感谢您对本小程序的支持！！！");
                    HttpContext.Current.Response.Write(XML);
                    HttpContext.Current.Response.End();
                }
                
                //获取开发者和用户的openid
                WxXmlModels.FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText;
                dynamic staff = WxappService.ZXKH_SelectShip(WxXmlModels.FromUserName);
                if (staff != null)
                {
                    WxXmlModels.ToUserName = (string)staff["staffid"];
                    if ((string)staff["XCXOPENID"] != null)
                    {
                        WxXmlModels.XCXFromOpenId = (string)staff["XCXOPENID"];
                        WxXmlModels.XCXToOpenId = (string)staff["staffxcxid"];
                    }
                }
                else
                {
                    WxXmlModels.ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText;
                }
                
                //if (WxappService.ZXKH_SelectShip(WxXmlModels.FromUserName).Result.ToString() == "" || WxappService.ZXKH_SelectShip(WxXmlModels.FromUserName).Result.ToString() == null)
                //{
                //    WxXmlModels.ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText;
                //}
                //else
                //{
                //    WxXmlModels.ToUserName = WxappService.ZXKH_SelectShip(WxXmlModels.FromUserName).Result.ToString();
                //}

                WxXmlModels.MsgType = rootElement.SelectSingleNode("MsgType").InnerText;

                try
                {
                    //关注后自动回复小程序链接
                    WxappService.ZXKH_WriteTxt(rootElement.SelectSingleNode("Event").InnerText);
                    WxXmlModels.Event = rootElement.SelectSingleNode("Event").InnerText;

                    if (WxXmlModels.Event == "subscribe")//关注类型
                    {
                        var a = WxappService.ZXKH_GetText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, WxXmlModels.MsgType);
                        HttpContext.Current.Response.Write(a);
                        HttpContext.Current.Response.End();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (WxXmlModels.MsgType == "text")
                    {
                        //获取内容
                        WxappService.ZXKH_WriteTxt(rootElement.SelectSingleNode("Content").InnerText);
                        WxXmlModels.CreateTime = long.Parse(rootElement.SelectSingleNode("CreateTime").InnerText);
                        WxXmlModels.Content = rootElement.SelectSingleNode("Content").InnerText;
                        WxXmlModels.MsgId = long.Parse(rootElement.SelectSingleNode("MsgId").InnerText);
                        

                        //Regex rg = new Regex("^[\u4e00-\u9fa5].*$");

                        //if (WxXmlModels.Content.Trim() == "确定")
                        //{

                        //    var XML = WxappService.ZXKH_ReText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, "恭喜你" +
                        //            "完成了信息填写，可以进入我们的小程序！☺☺☺ http://www.shuziyu.net:8189/ ");
                        //    WxappService.ZXKH_WriteTxt(XML);
                        //    HttpContext.Current.Response.Write(XML);
                        //    HttpContext.Current.Response.End();
                        //}
                        //if (WxXmlModels.Content.Trim() == "修改")
                        //{
                        //    WxappService.ZXKH_KHDel(WxXmlModels.FromUserName);
                        //    var XML = WxappService.ZXKH_ReText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, "请填写你需要修改的手机号码！");
                        //    HttpContext.Current.Response.Write(XML);
                        //    HttpContext.Current.Response.End();
                        //}

                        ////保存用户的消息和个人信息
                        //if (!WxappService.ZXKH_count(WxXmlModels.FromUserName))
                        //{
                        //    if (Regex.IsMatch(WxXmlModels.Content, @"^0?[1][3-9]\d{9}$"))
                        //    {
                        //        WxappService.ZXKH_BindCustomers(WxXmlModels.FromUserName, WxXmlModels.Content.Trim());
                        //        var XML = WxappService.ZXKH_ReText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, "请核对你的号码" + WxXmlModels.Content.Trim()
                        //           + "是否为正确信息！无错误请<a href=\"weixin://bizmsgmenu?msgmenucontent=确定&msgmenuid=10001\">确定</a>\n<a href=\"weixin://bizmsgmenu?msgmenucontent=修改&msgmenuid=10002\">修改</a>");
                        //        HttpContext.Current.Response.Write(XML);
                        //        HttpContext.Current.Response.End();

                        //        bl = true;
                        //    }
                        //    else if (rg.IsMatch(WxXmlModels.Content))
                        //    {
                        //        //号码错误回复客户
                        //        var erroe = WxappService.ZXKH_ReText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, "我们还没有记录你的手机号码，请输入手机号码！");
                        //        HttpContext.Current.Response.Write(erroe);
                        //        HttpContext.Current.Response.End();
                        //        bl = true;
                        //    }
                        //    else
                        //    {
                        //        //号码错误回复客户
                        //        var erroe = WxappService.ZXKH_ReText(WxXmlModels.FromUserName, WxXmlModels.ToUserName, "你输入的手机号码不正确，请重新输入！");
                        //        HttpContext.Current.Response.Write(erroe);
                        //        HttpContext.Current.Response.End();
                        //        bl = true;
                        //    }
                        //}
                    }
                    if (WxXmlModels.MsgType == "image")
                    {
                        WxXmlModels.CreateTime = long.Parse(rootElement.SelectSingleNode("CreateTime").InnerText);
                        WxXmlModels.PicUrl = rootElement.SelectSingleNode("PicUrl").InnerText;
                        WxXmlModels.MediaId = rootElement.SelectSingleNode("MediaId").InnerText;
                        WxXmlModels.MsgId = long.Parse(rootElement.SelectSingleNode("MsgId").InnerText);
                        WxXmlModels.Content = WxappService.ZXKH_MediaIdRequestImg(WxXmlModels.MediaId);
                    }
                    if (WxXmlModels.MsgType == "voice")
                    {
                        WxXmlModels.CreateTime = long.Parse(rootElement.SelectSingleNode("CreateTime").InnerText);
                        WxXmlModels.MediaId = rootElement.SelectSingleNode("MediaId").InnerText;
                        WxXmlModels.MsgId = long.Parse(rootElement.SelectSingleNode("MsgId").InnerText);
                        WxXmlModels.Format = rootElement.SelectSingleNode("Format").InnerText;
                        WxXmlModels.Recognition = rootElement.SelectSingleNode("Recognition").InnerText;
                        WxXmlModels.Content = WxappService.ZXKH_MediaIdRequestData(WxXmlModels.MediaId);

                    }
                    WxappService.ZXKH_WriteTxt(rootElement.ToString());
                    if (!bl)
                    {
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.ToUserName);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.FromUserName);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.CreateTime.ToString());
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.MsgType);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.MediaId);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.Format);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.Recognition);
                        //WxappService.ZXKH_WriteTxt(WxXmlModels.MsgId.ToString());
                        if (WxXmlModels.MsgType == "image" || WxXmlModels.MsgType == "voice" || WxXmlModels.MsgType == "text")
                        {
                            WxappService.ZXKH_savemessage(WxXmlModels);//用户发送信息保存到数据库

                            //if (WxappService.ZXKH_Staff_Customers(WxXmlModels.FromUserName)) //判断该用户是否有客服关联，微信公众号消息发送进行人员的第一次分配
                            //{
                            //    WxappService.ZXKH_AddShip(WxXmlModels.FromUserName, WxXmlModels.MsgId, WxXmlModels.MsgType);//添加
                            //    WxappService.ZXKH_WriteTxt("添加哦");
                            //}
                        }
                        
                        //else
                        //{
                        //    WxappService.EditShip(WxXmlModels.FromUserName);//修改
                        //    WxappService.WriteTxt("修改哦");
                        //}

                        HttpContext.Current.Response.Write(null);
                        HttpContext.Current.Response.End();
                    }
                }
            }
            //WebTool.WriteLog("微信请求", "请求JSON内容", requestStr);
            //WebTool.ResponseMsg(requestStr);//调用消息适配器
            //用户发送信息保存到数据库
        }

        /// <summary>
        /// 微信小程序-用户发送消息给用户，包括到公众号
        /// </summary>
        /// <param name="obj">用户发送的消息包</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("zxkh_sendMsg")]
        [HttpPost]
        [Transaction]
        public Response ZXKH_sendMsg([FromBody]CustomerServiceMessage obj)
         {
            if (false)
            {
                // 获取客服 obj.FromUserName == "-1" && obj.XCXToOpenId.Length > 28
                //obj.XCXFromOpenId = WxappService.pc_kf_Lastgroup(obj.XCXToOpenId);

                
                obj.CreateTime = WxappService.ZXKH_ConvertDateTimeInt(DateTime.Now);
               
                WxappService.ZXKH_savemessage(obj);

                //if (obj.XCXToOpenId.Length != 28)
                //{
                    var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                    hub.Clients.All.notify(WxappService.pc_Que(obj.XCXToOpenId));
                //}
                //else
                //{
                //    var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                //    hub.Clients.All.notify(WxappService.pc_QueryCustomerInfo(obj.ToUserName));
                //}
                //hub.Clients.All("1111");

                return new Response
                {
                    Result = new
                    {
                        obj
                    }
                }; 
            }
            else
            {
                // 时间戳
                 obj.CreateTime = WxappService.ZXKH_ConvertDateTimeInt(DateTime.Now);
                if (obj.MsgType == "text")
                {
                    if (obj.ToUserName != null)
                    {
                        //dynamic user = WxappService.ZXKH_OpenidToName(obj.XCXFromOpenId);
                        //WxappService.ZXKH_SendText(obj.ToUserName, obj.Content, (string)user["KHNAME"]);
                    }
                }
                if (obj.MsgType == "voice")
                {
                    if (obj.ToUserName != null)
                    {
                        dynamic user = WxappService.ZXKH_OpenidToName(obj.XCXFromOpenId);
                        //var media_id = WxappService.ZXKH_UploadImgByB64(obj.Content);
                        //WxappService.ZXKH_SendVoice(obj.ToUserName, media_id);
                        WxappService.ZXKH_SendVoice(obj.ToUserName, user["KHNAME"]);
                    }
                    obj.Content = WxappService.ZXKH_Base64ToFile(obj.Content, ".mp3");
                }
                if (obj.MsgType == "image")
                {
                    if (obj.ToUserName != null)
                    {
                        dynamic user = WxappService.ZXKH_OpenidToName(obj.XCXFromOpenId);
                        //var media_id = WxappService.ZXKH_UploadImgByB64(obj.Content);
                        //WxappService.ZXKH_SendImage(obj.ToUserName, media_id);
                        WxappService.ZXKH_SendImage(obj.ToUserName, user["KHNAME"]);
                    }
                    obj.Content = WxappService.ZXKH_Base64ToFile(obj.Content, ".jpg");
                }
                WxappService.ZXKH_savemessage(obj);

                //获取发送者的头像  暂时注释
                //var results = WxappService.ZXKH_QueryPicture(obj.XCXFromOpenId);
                //obj.PicUrl = (string)results["PICTURE"];
                //obj.KHNAME = (string)results["KHNAME"];
                var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                hub.Clients.All.notify(obj);
                //var staff = WxappService.ZXKH_QueryFID(obj.XCXToOpenId);
                //if (staff != null)
                //{
                //    string fid = staff["FID"].ToString();
                //    WxappService.ZXKH_WriteTxt("获取用户的fid");
                //    WxappService.ZXKH_WriteTxt(fid);
                //    dynamic result = WxappService.ZXKH_SendWebSocket(fid);
                //    if (result != null)
                //    {
                //        var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                //        var users = (string)result["ConnectionID"];
                //        WxappService.ZXKH_WriteTxt("获取用户的ConnectionID");
                //        WxappService.ZXKH_WriteTxt(users);
                //        //hub.Clients.Client(users).notify(obj);
                //        hub.Clients.All.notify(obj);
                //    }
                //}
                //else
                //{
                //    var group_connectionid = WxappService.ZXKH_GroupQueryFID(obj.XCXToOpenId);
                //    if (group_connectionid.Count != 0)
                //    {
                //        var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                //        hub.Clients.Clients(group_connectionid).notify(obj);

                //    }
                //}

                //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                //var data = new
                //{
                //    name = 1,
                //    age = 2
                //};
                //hub.Clients.All(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                //hub.Clients.All("1111");


                return new Response
                {
                    Result = new
                    {
                        obj
                    }
                };
            }

        }

        /// <summary>
        /// 微信公众号带参进入h5界面
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("webtest")]
        [HttpPost]
        [Transaction]
        public Response ZXKH_hhtest(string code)
        {
            
            return WxappService.ZXKH_wxgzh(code);
        }
        /// <returns></returns>
        [AllowAnonymous]
        [Route("gzhdata")]
        [HttpPost]
        [Transaction]
        public string ZXKH_WxgzhAuth(string url)
        {
            return WxappService.ZXKH_WxgzhAuth(url);
        }
        [AllowAnonymous]
        [Route("zxkh_gzhpath")]
        [HttpPost]
        [Transaction]
        public int ZXKH_gzhpath(string openid)
        {
            return WxappService.ZXKH_gzhpath(openid);
        }
        [HttpPost]
        [Route("upload/file")]
        public Response UploadFile()
        {
            //return WxappService.UploadFile();
            var result = WxappService.pc_UploadServiceImg();
            var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //hub.Clients.All.notify(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            hub.Clients.All.notify(result.Result);
            return result;
        }
        public string bbb()
        {
            return "以上新加";
        }
        

        /// <summary>
        /// pc 我的用户消息列表
        /// </summary>
        /// <returns></returns>
        [Route("pc_customers")]
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        public async Task<Response> pc_QueryCustomers([FromUri] string xcxopenid)
        {
            var result = await WxappService.pc_QueryCustomers(xcxopenid);

            return result;
        }

        /// <summary>
        /// pc所有群组列表
        /// </summary>
        /// <returns></returns>
        [Route("pc_zxkh_allgrouplist")]
        [HttpGet]
        [Transaction]
        [AllowAnonymous]
        public async Task<Response> pc_ZXKH_ALLGroupList()
        {
            var result = await WxappService.pc_ZXKH_ALLGroupList();

            return result;
        }

        /// <summary>
        /// 发送消息给用户
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("pc_zxkh_sendMsg")]
        [HttpPost]
        [Transaction]
        public Response pc_ZXKH_sendMsg([FromBody]CustomerServiceMessage obj)
        {


            //if (obj.XCXToOpenId.Length != 28)
            //{
            //    obj.XCXFromOpenId = WxappService.pc_kf_Lastgroup(obj.XCXToOpenId);
            //}
            obj.CreateTime = WxappService.ZXKH_ConvertDateTimeInt(DateTime.Now);
            if (obj.MsgType == "text")
            {
                //if (obj.ToUserName != null)
                //{
                //WxappService.ZXKH_SendText(obj.ToUserName, obj.Content);
                //}
            }
            WxappService.ZXKH_savemessage(obj);

            if (obj.XCXToOpenId.Length != 28)
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                hub.Clients.All.notify(WxappService.pc_Que(obj.XCXToOpenId));
            }
            else
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                hub.Clients.All.notify(WxappService.pc_QueryCustomerInfo(obj.ToUserName));
            }
            //hub.Clients.All("1111");

            return new Response
            {
                Result = new
                {
                    obj
                }
            };
        }

        /// <summary>
        /// 查询客户聊天记录
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Route("pc_customerMsg")]
        [HttpGet]
        [Transaction]
        public async Task<IList<CustomerServiceMessageVO>> pc_QueryCustomerMsg([FromUri]string wxopenid, [FromUri]int page, [FromUri]int limit)
        {
            //var relation_KF = WxappDao.pc_ZXKH_QueryGroupMsg(wxopenid, page, limit);
            var result = await WxappService.pc_QueryCustomerMsg(wxopenid, page, limit);

            return result;
        }

        /// <summary>
        /// 查询当前群聊聊天记录
        /// </summary>
        /// <param name="wxopenid"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Route("pc_zxkh_groupMsg")]
        [AllowAnonymous]
        [HttpGet]
        [Transaction]
        public async Task<Response> pc_ZXKH_QueryGroupMsg([FromUri]string wxopenid, [FromUri]int page, [FromUri]int limit)
        {
            var result = await WxappService.pc_ZXKH_QueryGroupMsg(wxopenid, page, limit);
            //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //hub.Clients.All.notify(result);
            return result;
        }

        /// <summary>
        /// 上传图片或者文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("pc_upload/service")]
        public Response pc_UploadServiceImg()
        {
            return WxappService.pc_UploadServiceImg();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("pc_everyoneCustomers")]
        public async Task<Response> pc_everyoneCustomers([FromUri]string userId)
        {
            return await WxappService.pc_everyoneCustomers(userId);
        }

        /// <summary>
        /// 监听显示--查询客服
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("pc_kfSelect")]
        public async Task<Response> pc_kfSelect([FromUri]string id)
        {
            return await WxappService.pc_GetKfSelect(id);
        }
        /// <summary>
        /// 监听显示 --添加
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Fid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("pc_kfRelation")]
        public Response kfRelation([FromUri]string id, [FromUri] string Fid)
        {
            return WxappService.kfRelation(id, Fid);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Management")]
        public IHttpActionResult Management ([FromUri]string userId)
        {
            return Ok(WxappService.Management(userId));
        }

        /// <summary>
        /// 添加为个人管理
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Transaction]
        [Route("Addition/update")]
        public Response permissions([FromUri]string khname, [FromUri]string fid, [FromUri]string fmobile, [FromUri]string xcxopenid,[FromUri]string ffid, [FromUri]string userId)
        {
            return new Response
            {
                Result = WxappService.permissions(khname, fid, fmobile, xcxopenid, ffid, userId)
            };
        }

        /// <summary>
        /// 删除不活跃的用户
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Transaction]
        [Route("DelSurplus")]
        public Response DelSurplus([FromBody]long[] pers,[FromUri]string userId)
        {
            return new Response
            {
                Result = WxappService.DelSurplus(pers,userId)
            };
        }
    }
}
