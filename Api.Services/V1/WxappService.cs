using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Api.Dao.V1;
using Api.Model.BO;
using Api.Model.DO;
using Api.Model.VO;
using Api.Model.VO.WX;
using Common.Utils;
using Common.WxService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;
using SZY.Common.Plugin.Tencent;
using Unity.Attributes;
using System.DrawingCore;
using NReco.VideoConverter;

namespace Api.Services.V1
{
    /**********************************************************************
	*创 建 人：
	*创建时间：2019-12-11 09:57:31
	*描  述  ：
	***********************************************************************/

    public class WxappService
    {
        private static readonly string AppId = ConfigurationManager.AppSettings["wxAppId"];
        private static readonly string Secret = ConfigurationManager.AppSettings["wxSecret"];
        private static readonly string MchId = ConfigurationManager.AppSettings["mchId"];
        private static readonly string MchKey = ConfigurationManager.AppSettings["mchKey"];
        private static readonly string WxAppMsgPullToken = ConfigurationManager.AppSettings["wxAppMsgPulToken"];

        private static readonly string QywxCorpId = ConfigurationManager.AppSettings["qywxCorpId"];
        private static readonly string QywxSecret = ConfigurationManager.AppSettings["qywxSecret"];
        private static readonly string QywxMsgPullToken = ConfigurationManager.AppSettings["qywxMsgPullToken"];
        private static readonly string QywxEncodingAESKey = ConfigurationManager.AppSettings["qywxEncodingAESKey"];

        private static readonly string Kd100Customer = ConfigurationManager.AppSettings["kd100_customer"];
        private static readonly string Kd100Key = ConfigurationManager.AppSettings["kd100_key"];

        private static readonly string UPLOAD_PHOTO_PATH = "upload/wx/custom/";
        private static readonly string UPLOAD_AVATAR_PATH = "upload/wx/avatar/";
        private static readonly string CREATE_XCXCODE_PATH = "upload/wx/xcxcode/";
        private static readonly string SERVICE_IMG_PATH = "upload/service/";

        private static readonly string gzhAppId = ConfigurationManager.AppSettings["gzhAppId"];
        private static readonly string gzhSecret = ConfigurationManager.AppSettings["gzhSecret"];

        /// <summary>
        /// 事件类型消息
        /// </summary>
        private static readonly string TENCENT_MSG_TYPE_EVENT = "event";

        /// <summary>
        /// 小程序卡片类型消息
        /// </summary>
        private static readonly string TENCENT_MSG_TYPE_CARD = "miniprogrampage";

        /// <summary>
        /// 图片类型消息
        /// </summary>
        private static readonly string TENCENT_MSG_TYPE_IMG = "image";

        private static readonly object locker = new object();

        [Dependency]
        public WxappDao WxappDao
        {
            get;
            set;
        }




        /// <summary>
        /// 查询分类下商品
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Response QueryGoodsByType(GoodsFilterVO filter)
        {
            IList<dynamic> goods = null;
            if (filter.Type == "定制")
            {
                goods = WxappDao.QueryCusableGoods();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(filter.Kw))
                {
                    filter.Kw = string.Empty;
                }
                goods = WxappDao.QueryGoodsByType(filter);
                //遍历查询活动金额
                foreach (var item in goods)
                {
                    item["Activity"] = WxappDao.QueryGoodsActivity((int)item["FID"]);
                }
                if (filter.Category == 1)
                {
                    goods = goods.Where(x => x["Activity"] != null).ToList();
                }
            }
            return new Response
            {
                Result = goods
            };
        }

        /// <summary>
        /// 查询用户待付款、待发货、待收货的订单数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Response QueryOrderCountStatusNum(int uid)
        {
            var result = WxappDao.QueryOrderCountStatusNum(uid).First();
            return new Response
            {
                Result = result
            };
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
        public Response QueryOrders(int uid, string status, string kw, int page, int pageSize)
        {
            IList<dynamic> result = null;
            if (status == "待付款")
            {
                result = WxappDao.QueryWaitingPayOrders(uid, kw, page, pageSize);
                //副表
                if (result.Any())
                {
                    var ids = result.Select(x => (int)x["FID"]).ToList<int>();
                    var entryList = WxappDao.QueryAllOrdersEntry(ids.ToArray());

                    foreach (var item in result)
                    {
                        item["entries"] = entryList.Where(x => (int)x["FID"] == (int)item["FID"]).ToList();
                    }
                }
            }
            else if (status == "待发货")
            {
                result = WxappDao.QueryWaitingSendOrders(uid, kw, page, pageSize);
                //副表
                if (result.Any())
                {
                    var ids = result.Select(x => (int)x["FID"]).ToList<int>();
                    var entryList = WxappDao.QueryAllOrdersEntry(ids.ToArray());

                    foreach (var item in result)
                    {
                        item["entries"] = entryList.Where(x => (int)x["FID"] == (int)item["FID"]).ToList();
                    }
                }
            }
            else if (status == "待收货")
            {
                result = WxappDao.QueryWaitingSignOrders(uid, kw, page, pageSize);
                //副表
                if (result.Any())
                {
                    var ids = result.Select(x => (int)x["FID"]).ToList<int>();
                    var entryList = WxappDao.QueryAllOrdersEntry(ids.ToArray());

                    foreach (var item in result)
                    {
                        item["entries"] = entryList.Where(x => (int)x["FID"] == (int)item["FID"]).ToList();
                    }
                }
            }
            else
            {
                //全部订单
                //主表
                result = WxappDao.QueryAllOrders(uid, kw, page, pageSize);
                //副表
                if (result.Any())
                {
                    var ids = result.Select(x => (int)x["FID"]).ToList<int>();
                    var entryList = WxappDao.QueryAllOrdersEntry(ids.ToArray());

                    foreach (var item in result)
                    {
                        item["entries"] = entryList.Where(x => (int)x["FID"] == (int)item["FID"]).ToList();
                    }
                }
            }

            return new Response
            {
                Result = result
            };
        }

        public Response QueryShares(string wxopenid, int page, int pageSize)
        {
            IList<dynamic> result = null;

            result = WxappDao.QueryCustomerShareInfo(wxopenid, page, pageSize);
            //副表
            if (result.Any())
            {
                var ids = result.Select(x => (int)x["GOODSID"]).ToList<int>();

                var entryList = WxappDao.QueryGoodsImage(ids.ToArray());

                foreach (var item in result)
                {
                    item["entries"] = entryList.Where(x => (int)x["FID"] == (int)item["GOODSID"]).ToList();
                }
            }

            return new Response
            {
                Result = result
            };
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单id</param>
        /// <returns></returns>
        public Response CancelOrder(int id)
        {
            var result = WxappDao.CancelOrder(id);
            return new Response
            {
                Result = result
            };
        }

        /// <summary>
        /// 查询指定商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response QueryGoodsById(int id)
        {
            IList<dynamic> goods = WxappDao.QueryPublishGoodsById(id);
            //遍历查询活动金额
            foreach (var item in goods)
            {
                item["image"] = WxappDao.QueryGoodsImages((int)item["FID"]);

                var activity = WxappDao.QueryGoodsActivity((int)item["FID"]);
                if (activity != null)
                {
                    var startTimeSpan = ((DateTime)activity["FSTARTDATE"]).Subtract(DateTime.Now);
                    var endTimeSpan = ((DateTime)activity["FENDDATE"]).Subtract(DateTime.Now);
                    activity["startTimeSpan"] = startTimeSpan.TotalMilliseconds;
                    activity["endTimeSpan"] = endTimeSpan.TotalMilliseconds;
                }
                item["Activity"] = activity;

                var sku = WxappDao.QueryGoodsSku((int)item["FID"]);
                item["Sku"] = sku;
            }

            return new Response
            {
                Result = goods.FirstOrDefault()
            };
        }

        /// <summary>
        /// 上传定制图片
        /// </summary>
        /// <returns></returns>
        public Response UploadCusPhoto()
        {
            var files = HttpContext.Current.Request.Files;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string filePath = UPLOAD_PHOTO_PATH + DateTime.Now.ToString("yyyyMMdd");
            int limitFileSize = 1024 * 1024 * 10;

            string fullPath = basePath + filePath;
            string savePath = "";

            //如果目录不存在，则创建目录
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (files.Count > 0)
            {
                foreach (var key in files.AllKeys)
                {
                    var file = files[key];
                    //校验文件类型
                    string fileExtension = Path.GetExtension(file.FileName);
                    string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                    string[] fileTypeWhiteList = new string[] { ".jpg", ".jpeg", ".png" };
                    string[] fileMimeTypeWhiteList = new string[] { "image/jpg", "image/jpeg", "image/png" };
                    if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                    {
                        throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                    }

                    if (file.ContentLength > limitFileSize)
                    {
                        throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                    }

                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                        savePath = filePath + "/" + fileName;
                        file.SaveAs(fullPath + "/" + fileName);
                    }
                }
                return new Response
                {
                    Result = savePath
                };
            }
            else
            {
                throw new Exception("上传失败，未接收到请求文件！");
            }
        }

        public Response UploadServiceImg()
        {
            var files = HttpContext.Current.Request.Files;
            var form = HttpContext.Current.Request.Form;
            CustomerServiceMessage customerService = new CustomerServiceMessage
            {
                MsgType = form["msgType"],
                FromUserName = form["fromUserName"],
                ToUserName = form["toUserName"],
                Content = "",
                AppId = "",
                CreateTime = TimeStampHelper.ToTimeStamp(DateTime.Now)
            };

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string filePath = SERVICE_IMG_PATH + DateTime.Now.ToString("yyyyMMdd");
            int limitFileSize = 1024 * 1024 * 2;

            string fullPath = basePath + filePath;
            string savePath = "";

            //如果目录不存在，则创建目录
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (files.Count > 0)
            {
                foreach (var key in files.AllKeys)
                {
                    var file = files[key];
                    //校验文件类型
                    string fileExtension = Path.GetExtension(file.FileName);
                    string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                    string[] fileTypeWhiteList = new string[] { ".jpg", ".jpeg", ".png" };
                    string[] fileMimeTypeWhiteList = new string[] { "image/jpg", "image/jpeg", "image/png" };
                    if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                    {
                        throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                    }

                    if (file.ContentLength > limitFileSize)
                    {
                        throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                    }

                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                        savePath = filePath + "/" + fileName;
                        file.SaveAs(fullPath + "/" + fileName);

                        var token = WxHelper.GetAccessToken(AppId, Secret);
                        var mediaId = TencentHelper.UploadTempMedia(token, fullPath + "/" + fileName);
                        customerService.MediaId = mediaId;
                        customerService.Image = new
                        {
                            media_id = mediaId
                        };
                        customerService.PicUrl = savePath;
                        WxappDao.SaveCustomerMessage(customerService);

                        TencentHelper.SendCustomerMessageToUser(token, TencentHelper.MSG_TYPE_IMG, string.Empty, mediaId, customerService.ToUserName);
                    }
                }
                return new Response
                {
                    Result = savePath
                };
            }
            else
            {
                throw new Exception("上传失败，未接收到请求文件！");
            }
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public Response UploadAvatar(int id)
        {
            var files = HttpContext.Current.Request.Files;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string filePath = UPLOAD_AVATAR_PATH;
            int limitFileSize = 1024 * 1024 * 10;

            string fullPath = basePath + filePath;

            string relativePath = string.Empty;

            //如果目录不存在，则创建目录
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (files.Count > 0)
            {
                foreach (var key in files.AllKeys)
                {
                    var file = files[key];
                    //校验文件类型
                    string fileExtension = Path.GetExtension(file.FileName);
                    string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                    string[] fileTypeWhiteList = new string[] { ".jpg", ".jpeg", ".png" };
                    string[] fileMimeTypeWhiteList = new string[] { "image/jpg", "image/jpeg", "image/png" };
                    if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                    {
                        throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                    }

                    if (file.ContentLength > limitFileSize)
                    {
                        throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                    }

                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        string guid = Guid.NewGuid().ToString("N");
                        string fileName = guid + Path.GetExtension(file.FileName);
                        string thumbName = string.Format("{0}_thumb{1}", guid, Path.GetExtension(file.FileName));

                        string absolutePath = fullPath + "/" + fileName;
                        string thumbAbsolutePath = fullPath + "/" + thumbName;
                        relativePath = filePath + "/" + thumbName;

                        file.SaveAs(absolutePath);
                        ImageHelper.GetReducedImage(150, 150, absolutePath, thumbAbsolutePath);
                        File.Delete(absolutePath);

                        var staffAvatar = WxappDao.QueryStaffAvatar(id);
                        if (staffAvatar == null)
                        {
                            ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
                            {
                                Picture = relativePath,
                                UseWxAvatar = false,
                                StaffId = id
                            };
                            WxappDao.AddOrUpdateStaffAvatar(avatar);
                        }
                        else
                        {
                            staffAvatar.Picture = relativePath;
                            staffAvatar.UseWxAvatar = false;
                            WxappDao.AddOrUpdateStaffAvatar(staffAvatar);
                        }
                    }
                }
                return new Response
                {
                    Result = relativePath
                };
            }
            else
            {
                throw new Exception("上传失败，未接收到请求文件！");
            }
        }

        /// <summary>
        /// 使用微信头像
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public Response UseWxAvatar(ESSChannelStaffAvatar avatar)
        {
            var staffAvatar = WxappDao.QueryStaffAvatar(avatar.StaffId);
            if (staffAvatar == null)
            {
                avatar.UseWxAvatar = true;
                WxappDao.AddOrUpdateStaffAvatar(avatar);
            }
            else
            {
                staffAvatar.UseWxAvatar = true;
                staffAvatar.Picture = avatar.Picture;
                avatar = null;
                WxappDao.AddOrUpdateStaffAvatar(staffAvatar);
            }
            return new Response
            {
                Result = 1
            };
        }

        public Response QueryUseAvatar(int id)
        {
            var customer = WxappDao.QueryCustomer(id);
            var staffAvatar = WxappDao.QueryStaffAvatar(id);

            return new Response
            {
                Result = new
                {
                    avatarUrl = staffAvatar.Picture,
                    nickName = customer.KHNAME
                }
            };
        }

        /// <summary>
        /// 查询所有已发布活动
        /// </summary>
        /// <returns></returns>
        public Response QueryAllPublishActivity()
        {
            var result = WxappDao.QueryAllPublishActivity();
            return new Response
            {
                Result = result
            };
        }


       


        /// <summary>
        /// 查询用户购物车
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Response QueryUserCart(int uid)
        {
            IList<dynamic> goods = WxappDao.QueryUserCart(uid);
            //遍历查询活动金额
            foreach (var item in goods)
            {
                if (item["ZC"] != null && (string)item["ZC"] == "SKU")
                {
                    item["Activity"] = null;
                }
                else
                {
                    var activity = WxappDao.QueryGoodsActivity((int)item["FID"]);
                    if (activity != null)
                    {
                        var startTimeSpan = ((DateTime)activity["FSTARTDATE"]).Subtract(DateTime.Now);
                        var endTimeSpan = ((DateTime)activity["FENDDATE"]).Subtract(DateTime.Now);
                        if (startTimeSpan.TotalMilliseconds <= 0 && endTimeSpan.TotalMilliseconds > 0)
                        {
                            activity["InActivity"] = true;
                        }
                        else
                        {
                            activity["InActivity"] = false;
                        }
                    }
                    item["Activity"] = activity;
                }
            }

            return new Response
            {
                Result = goods
            };
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Response QueryUserInfo(int uid)
        {
            var result = WxappDao.QueryUserInfo(uid);

            return new Response
            {
                Result = result
            };
        }

        public Response UpdateUserGender(int uid, JObject jObject)
        {
            var gender = jObject["gender"].ToString();
            var result = WxappDao.UpdateUserGender(uid, gender);

            var user = WxappDao.QueryUserInfo(uid);

            var data = new
            {
                memberPhone = (string)user["FMOBILE"],
                gender = jObject["gender"].ToString()
            };
            //A3Service.UpdateCustomerInfo(data);

            return new Response
            {
                Result = result
            };
        }

        public Response UpdateUserBirthday(int uid, JObject jObject)
        {
            var birthday = jObject["birthday"].ToString();
            var result = WxappDao.UpdateUserBirthday(uid, birthday);
            var user = WxappDao.QueryUserInfo(uid);

            var data = new
            {
                memberPhone = (string)user["FMOBILE"],
                birthday = jObject["birthday"].ToString()
            };
            //A3Service.UpdateCustomerInfo(data);
            return new Response
            {
                Result = result
            };
        }

        public Response UpdateUserArea(int uid, JObject jObject)
        {
            var area = jObject["area"].ToString();
            var result = WxappDao.UpdateUserArea(uid, area);

            var user = WxappDao.QueryUserInfo(uid);

            var data = new
            {
                memberPhone = (string)user["FMOBILE"],
                area = jObject["area"].ToString()
            };
            //A3Service.UpdateCustomerInfo(data);

            return new Response
            {
                Result = result
            };
        }

        public Response UpdateUserName(int uid, JObject jObject)
        {
            var name = jObject["name"].ToString();
            var result = WxappDao.UpdateUserName(uid, name);

            var user = WxappDao.QueryUserInfo(uid);

            var data = new
            {
                memberPhone = (string)user["FMOBILE"],
                name = jObject["name"].ToString()
            };
            //A3Service.UpdateCustomerInfo(data);

            return new Response
            {
                Result = result
            };
        }

        public Response UpdateUserWechat(int uid, JObject jObject)
        {
            var wechat = jObject["wechat"].ToString();
            var result = WxappDao.UpdateUserWechat(uid, wechat);

            return new Response
            {
                Result = result
            };
        }

        /// <summary>
        /// 添加分享信息
        /// </summary>
        /// <param name="wxShare"></param>
        /// <returns></returns>
        public Response AddShareInfo(WxShareVO wxShare)
        {
            var shareInfo = new ShareInfo
            {
                ActivityId = wxShare.ActivityId,
                GoodsId = wxShare.GoodsId,
                Uuid = wxShare.Uuid
            };

            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, wxShare.Code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg == null)
            {
                var openid = (string)data.openid;
                shareInfo.Date = DateTime.Now;
                shareInfo.WxOpenId = openid;
                WxappDao.AddShareInfo(shareInfo);

                return new Response
                {
                    Result = 1
                };
            }
            else
            {
                throw new Exception((string)data.errmsg);
            }
        }

        /// <summary>
        /// 添加分享浏览信息
        /// </summary>
        /// <param name="wxShare"></param>
        /// <returns></returns>
        public Response AddShareInfoEntry(WxShareVO wxShare)
        {
            var shareInfoEntry = new ShareInfoEntry
            {
                Uuid = wxShare.Uuid
            };

            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, wxShare.Code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg == null)
            {
                var openid = (string)data.openid;
                shareInfoEntry.Date = DateTime.Now;
                shareInfoEntry.WxOpenId = openid;
                WxappDao.AddShareInfoEntry(shareInfoEntry);

                return new Response
                {
                    Result = 1
                };
            }
            else
            {
                throw new Exception((string)data.errmsg);
            }
        }

        /// <summary>
        /// 计算剩余返利
        /// </summary>
        /// <param name="memberPhone"></param>
        /// <returns></returns>
        public Response CalculationRebate(string memberPhone)
        {
            //int useRebate = WxappDao.QueryJrUseRebate(memberPhone);
            //var selectjrRebate = Convert.ToInt32(A3Service.QueryUserRebate(memberPhone).Result);
            //int RebateBalance = selectjrRebate - useRebate;
            
            return new Response
            {
                Result = 0
            };
        }

        public Response GetOpenId(string code)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, code);

            WxPayloadBO wxFirst = null;
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string msg = response.Content.ReadAsStringAsync().Result;
                wxFirst = JsonConvert.DeserializeObject<WxPayloadBO>(msg);
                wxFirst.session_key = "";
            }
            return new Response
            {
                Result = wxFirst
            };
        }

        /// <summary>
        /// 微信统一下单
        /// </summary>
        /// <param name="uniqOrder"></param>
        /// <returns></returns>
        public Response UnifiedOrder(UniqOrderVO uniqOrder)
        {
            var openid = uniqOrder.Openid;
            var orderNo = uniqOrder.OrderNo;
            var orderAmount = Math.Round(Convert.ToDecimal(uniqOrder.OrderAmount) * 100, 0, MidpointRounding.AwayFromZero);

            DateTime dateStart = new DateTime(1970, 1, 1, 0, 0, 0);

            var nonceStr = Guid.NewGuid().ToString("N");
            var notifyUrl = "https://znpz.net/api/wxapp/paynotify";
            var ip = GetHostAddress();

            WxUniqOrderBO order = new WxUniqOrderBO()
            {
                appid = AppId,
                body = "中酿品致名酒商城",
                mch_id = MchId,
                nonce_str = nonceStr,
                notify_url = notifyUrl,
                openid = openid,
                out_trade_no = orderNo,
                spbill_create_ip = ip,
                total_fee = orderAmount.ToString(),
                trade_type = "JSAPI",
                sign = ""
            };

            Type t = order.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            PropertyList = PropertyList.OrderBy(x => x.Name).ToArray();
            StringBuilder builder = new StringBuilder();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                string value = item.GetValue(order).ToString();
                if (value != "")
                {
                    builder.Append(name + "=" + value + "&");
                }
            }
            var stringSignTemp = builder.ToString().TrimEnd('&') + "&key=" + MchKey;
            //将字符串进行MD5运算
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bt = md5.ComputeHash(Encoding.UTF8.GetBytes(stringSignTemp));
            var singString = BitConverter.ToString(bt).Replace("-", "").ToUpper();
            order.sign = singString;

            HttpClient client = new HttpClient();
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";

            string xml = @"<xml>
<appid>{0}</appid>
<body>中酿品致名酒商城</body>
<mch_id>{1}</mch_id>
<nonce_str>{2}</nonce_str>
<notify_url>{3}</notify_url>
<openid>{4}</openid>
<out_trade_no>{5}</out_trade_no>
<spbill_create_ip>{6}</spbill_create_ip>
<total_fee>{7}</total_fee>
<trade_type>JSAPI</trade_type>
<sign>{8}</sign>
</xml>";
            xml = string.Format(xml, AppId, MchId, nonceStr, notifyUrl, openid, orderNo, ip, orderAmount.ToString(), singString);

            var content = new StringContent(xml, Encoding.UTF8, "application/xml");
            var response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string msg = response.Content.ReadAsStringAsync().Result;
                XmlSerializer serializer = new XmlSerializer(typeof(xml));
                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
                xml result = (xml)serializer.Deserialize(memStream);

                if (result.return_code == "FAIL")
                {
                    throw new HttpException(result.return_msg);
                }
                else
                {
                    WxResponseBO r = new WxResponseBO
                    {
                        timeStamp = Convert.ToInt32((DateTime.Now - dateStart).TotalSeconds).ToString(),
                        nonceStr = Guid.NewGuid().ToString("N"),
                        package = "prepay_id=" + result.prepay_id,
                        signType = "MD5"
                    };
                    var paySign = "appId=" + AppId + "&nonceStr=" + r.nonceStr + "&package=" + r.package + "&signType=" + r.signType + "&timeStamp=" + r.timeStamp + "&key=" + MchKey;
                    bt = md5.ComputeHash(Encoding.UTF8.GetBytes(paySign));
                    var paySignString = BitConverter.ToString(bt).Replace("-", "").ToUpper();
                    r.paySign = paySignString;
                    return new Response
                    {
                        Result = r
                    };
                }
            }
            else
            {
                throw new HttpException("统一下单失败");
            }
        }

       

        /// <summary>
        /// 订阅快递100物流信息
        /// </summary>
        /// <param name="orderNum"></param>
        /// <returns></returns>
        public Response Kd100Sub(string orderNum)
        {
            HttpClient client = new HttpClient();
            string url = "https://poll.kuaidi100.com/poll?schema=json&param={0}";

            var param = new
            {
                company = "debangwuliu",
                number = "748058772",
                key = Kd100Key,
                parameters = new
                {
                    callbackurl = "https://znpz.net/api/wxapp/kd100/recive",
                    //salt = BouncyCastleHashing.CreateSalt(),
                    resultv2 = "1",
                    autoCom = "1",
                    //phone=""//顺丰必填
                }
            };

            url = string.Format(url, JsonConvert.SerializeObject(param));
            HttpResponseMessage response = client.PostAsJsonAsync(url, new { }).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if ((bool)data.result)
            {
                return new Response
                {
                    Result = 1
                };
            }
            else
            {
                throw new Exception((string)data.message);
            }
        }

      

        /// <summary>
        /// 生成小程序码
        /// </summary>
        /// <returns></returns>
        public Response CreateXcxCode(string uuid)
        {
            HttpClient client = new HttpClient();
            string token = WxHelper.GetAccessToken(AppId, Secret);

            string url2 = $"https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token={token}";
            var obj = new
            {
                page = "pages/product/main",
                scene = uuid
            };

            HttpResponseMessage response = client.PostAsJsonAsync(url2, obj).Result;
            response.EnsureSuccessStatusCode();

            string fileFullPath = "";
            using (var stream = response.Content.ReadAsStreamAsync().Result)
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                string filePath = CREATE_XCXCODE_PATH + DateTime.Now.ToString("yyyyMMdd") + "/";

                if (!Directory.Exists(basePath + filePath))
                {
                    Directory.CreateDirectory(basePath + filePath);
                }
                string fileName = Guid.NewGuid().ToString("N") + ".jpeg";

                fileFullPath = filePath + fileName;

                System.Drawing.Image.FromStream(stream, false).Save(basePath + fileFullPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                return new Response
                {
                    Result = fileFullPath
                };
            }
        }

        /// <summary>
        /// 查询分享商品信息
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public Response QueryShareGoods(string uuid)
        {
            var result = WxappDao.QueryShareGoods(uuid);
            return new Response
            {
                Result = result
            };
        }

        public Response GetAccessToken()
        {
            var token = Common.WxService.WxHelper.GetAccessToken(AppId, Secret);
            return new Response
            {
                Result = token
            };
        }

   
        /// <summary>
        /// 消息推送验证
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <returns></returns>
        public string VerifyMsgPullUrl(string signature, string timestamp, string nonce, string echostr)
        {
            var result = TencentHelper.VerifyUrlSetting(signature, WxAppMsgPullToken, timestamp, nonce);

            if (result == 1)
            {
                return echostr;
            }
            else
            {
                return result.ToString();
            }
        }

        public string VerifyQyMsgPullUrl(string signature, string timestamp, string nonce, string echostr)
        {
            var result = TencentHelper.VerifyQywxUrlSetting(QywxEncodingAESKey, QywxCorpId, signature, QywxMsgPullToken, timestamp, nonce, echostr);

            if (result == "0")
            {
                return "error";
            }
            else
            {
                return result;
            }
        }

        public async Task<object> ReceiveCustomerMsg(CustomerServiceMessage serviceMessage)
        {
            if (serviceMessage == null)
            {
                throw new ArgumentNullException("客服消息包为NULL！");
            }
            else
            {
                if (serviceMessage.MsgType == TENCENT_MSG_TYPE_EVENT)
                {
                    return null;
                }
                else
                {
                    var token = WxHelper.GetAccessToken(AppId, Secret);

                    string basePath = AppDomain.CurrentDomain.BaseDirectory;

                    string filePath = SERVICE_IMG_PATH + DateTime.Now.ToString("yyyyMMdd");

                    string absFilePath = basePath + filePath;

                    if (!Directory.Exists(absFilePath))
                    {
                        Directory.CreateDirectory(absFilePath);
                    }

                    string fileName = $"{Guid.NewGuid():N}.jpeg";

                    string fullPath = absFilePath + $"/{fileName}";

                    if (serviceMessage.MsgType == TENCENT_MSG_TYPE_IMG)
                    {
                        serviceMessage.PicUrl = filePath + $"/{fileName}";

                        TencentHelper.DownloadTempMedia(token, serviceMessage.MediaId, fullPath);
                    }
                    if (serviceMessage.MsgType == TENCENT_MSG_TYPE_CARD)
                    {
                        serviceMessage.PicUrl = filePath + $"/{fileName}";

                        TencentHelper.DownloadTempMedia(token, serviceMessage.ThumbMediaId, fullPath);
                    }
                    await WxappDao.SaveCustomerMessage(serviceMessage);

                    var customers = WxappDao.QueryCustomerInfo(serviceMessage.FromUserName);

                    RedisHelper.StringIncrementSet(serviceMessage.FromUserName, 1);

                    foreach (var item in customers)
                    {
                        item["content"] = serviceMessage.MsgType == TENCENT_MSG_TYPE_IMG ? "[图片]" : serviceMessage.Content;
                        item["count"] = RedisHelper.StringGet(serviceMessage.FromUserName);
                    }

                    SendQywxAppMessage();

                    return customers.FirstOrDefault();
                }

                //var token = WxHelper.GetAccessToken(AppId, Secret);

                //var serviceUsers = TencentHelper.QueryServiceUserList<ServiceUser>(token);
                //TencentHelper.CreateServiceMessageSession(token, serviceUsers.First().kf_account, serviceMessage.FromUserName);

                //TencentHelper.SendCustomerMessageToUser(token, "hello everyone!", "oYzDd4tE_k5a6EpLUxiyu8Z8kdec");
            }
        }

        public async Task<IList<CustomerServiceMessageVO>> QueryCustomerMsg(string wxopenid, int page, int limit)
        {
            var result = WxappDao.QueryCustomerMessage(wxopenid, page, limit);

            IList<CustomerServiceMessageVO> vos = AutoMapper.Mapper.Map<List<CustomerServiceMessageVO>>(result);
            foreach (var item in vos)
            {
                if (item.MsgType == TENCENT_MSG_TYPE_CARD)
                {
                    string goodsId = item.PagePath.Substring(item.PagePath.IndexOf("=") + 1);
                    item.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
                }

                if (int.TryParse(item.FromUserName, out int userId))
                {
                    var user = WxappDao.QueryUserInfo(userId);
                    item.FromUserName = user["KHNAME"];
                    item.ThumbUrl = user["PICTURE"];
                }
            }
            return await Task.FromResult(vos);
        }

        public Task<CustomerServiceMessageVO> QueryCustomerLastMsg(string wxopenid)
        {
            var result = WxappDao.QueryCustomerLastMessage(wxopenid);

            var vo = AutoMapper.Mapper.Map<CustomerServiceMessageVO>(result);
            if (vo != null)
            {
                if (int.TryParse(vo.FromUserName, out int userId))
                {
                    var user = WxappDao.QueryUserInfo(userId);
                    vo.FromUserName = user["KHNAME"];
                    vo.ThumbUrl = user["PICTURE"];
                }

                if (vo.MsgType == TENCENT_MSG_TYPE_CARD)
                {
                    string goodsId = vo.PagePath.Substring(vo.PagePath.IndexOf("=") + 1);
                    vo.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
                }
            }
            return Task.FromResult(vo);
        }

        public Task<Response> UpdateCustomerServiceStaff(int id, WxPayloadVO wxPayload)
        {
            WxappDao.UpdateUserName(id, wxPayload.NickName);
            var staffAvatar = WxappDao.QueryStaffAvatar(id);
            staffAvatar.Picture = wxPayload.AvatarUrl;

            var result = new Response
            {
                Result = 1
            };
            return Task.FromResult(result);
        }

        public async Task<Response> QueryCustomers()
        {
            var result = WxappDao.QueryCustomers();

            foreach (var item in result)
            {
                var openid = (string)item["FWXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["createtime"]).AddHours(-8)).TotalMinutes;
            }

            //result = result.Where(x => x["diffMinutes"] <= 48 * 60).ToList();

            return new Response
            {
                Result = result
            };
        }

        public async Task<object> SendCustomerMsg(CustomerServiceMessage serviceMessage)
        {
            if (serviceMessage == null)
            {
                throw new ArgumentNullException("客服消息包为NULL！");
            }
            else
            {
                serviceMessage.AppId = "";
                serviceMessage.CreateTime = TimeStampHelper.ToTimeStamp(DateTime.Now);
                await WxappDao.SaveCustomerMessage(serviceMessage);

                IList<dynamic> customers = new List<dynamic>();

                if (int.TryParse(serviceMessage.FromUserName, out int userId))
                {
                    customers = WxappDao.QueryCustomerInfo(serviceMessage.ToUserName);

                    foreach (var item in customers)
                    {
                        item["content"] = serviceMessage.MsgType == TENCENT_MSG_TYPE_IMG ? "[图片]" : serviceMessage.Content;
                        item["count"] = "";
                    }
                }
                //var serviceUsers = TencentHelper.QueryServiceUserList<ServiceUser>(token);
                //TencentHelper.CreateServiceMessageSession(token, serviceUsers.First().kf_account, serviceMessage.FromUserName);

                if (int.TryParse(RedisHelper.StringGet(serviceMessage.ToUserName), out int count) && count > 0)
                {
                    RedisHelper.StringSet(serviceMessage.ToUserName, 0, null);
                }

                var token = WxHelper.GetAccessToken(AppId, Secret);
                TencentHelper.SendCustomerMessageToUser(token, TencentHelper.MSG_TYPE_TEXT, serviceMessage.Content, string.Empty, serviceMessage.ToUserName);

                return customers.FirstOrDefault();
            }
        }

        public Response QywxCreateChatAndSendMsg(string message)
        {
            var token = WxHelper.GetQywxAccessToken(QywxCorpId, QywxSecret);
            var chatId = "3";
            var chatName = "小程序客户张三咨询群";
            TencentHelper.CreateQywxChat(token, chatId, chatName);
            TencentHelper.SendQyexMsgToChat(token, chatId, TencentHelper.MSG_TYPE_TEXT, message, string.Empty);
            return new Response
            {
                Result = 1
            };
        }

        /// <summary>
        /// 企业微信发送小程序卡片消息
        /// </summary>
        /// <returns></returns>
        private void SendQywxAppMessage()
        {
            var token = WxHelper.GetQywxAccessToken(QywxCorpId, QywxSecret);
            var deptList = RedisHelper.StringGet("qywx_dept");
            if (string.IsNullOrWhiteSpace(deptList))
            {
                lock (locker)
                {
                    var depts = TencentHelper.QueryQywxDepts(token);
                    RedisHelper.StringSet("qywx_dept", JsonConvert.SerializeObject(depts), null);

                    IList<SZY.Common.Plugin.Tencent.Model.UserModel> ulist = new List<SZY.Common.Plugin.Tencent.Model.UserModel>();
                    foreach (var item in depts)
                    {
                        var data = TencentHelper.QueryQywxDeptMembers(token, item.Id);
                        ulist = ulist.Union(data).ToList();
                    }
                    RedisHelper.StringSet("qywx_userlist", JsonConvert.SerializeObject(ulist), null);
                }
            }

            var userlist = RedisHelper.StringGet<IList<SZY.Common.Plugin.Tencent.Model.UserModel>>("qywx_userlist");

            foreach (var item in userlist)
            {
                if (item.UserId == "YiChaoRuMeng")
                {
                    var contentItem = new List<dynamic>() { new { key = "重要", value = "商城客户发来一条消息，请尽快回复！" } };
                    TencentHelper.SendQywxAppMessage(AppId, token, item.UserId, "pages/chatcontact/main", "客户消息提醒", DateTime.Now.ToString("MM月dd日 HH:mm"), contentItem, false);
                }
            }
        }

        public Response QueryServiceUser(string token)
        {
            return new Response
            {
                Result = TencentHelper.QueryServiceUserList<ServiceUser>(token)
            };
        }

       

        /// <summary>
        /// 获取客户端IP地址（无视代理）
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        private string GetHostAddress()
        {
            HttpClient client = new HttpClient();
            string url = "http://ip-api.com/json";

            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string msg = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<dynamic>(msg);
                if ((string)result.status == "success")
                {
                    return (string)result.query;
                }
                else
                {
                    throw new Exception("验证支付接口Ip地址失败，请稍后重试！");
                }
            }
            else
            {
                throw new Exception("验证支付接口Ip地址失败，请稍后重试！");
            }
        }

        public string jjj()
        {
            return "以下新加";
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public Response ZXKH_UploadAvatar(int id)
        {
            var files = HttpContext.Current.Request.Files;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = UPLOAD_AVATAR_PATH;
            int limitFileSize = 1024 * 1024 * 10;

            string fullPath = basePath + filePath;

            string relativePath = string.Empty;

            //如果目录不存在，则创建目录
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (files.Count > 0)
            {
                foreach (var key in files.AllKeys)
                {
                    var file = files[key];
                    //校验文件类型
                    string fileExtension = Path.GetExtension(file.FileName);
                    string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                    string[] fileTypeWhiteList = new string[] { ".jpg", ".jpeg", ".png" };
                    string[] fileMimeTypeWhiteList = new string[] { "image/jpg", "image/jpeg", "image/png" };
                    if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                    {
                        throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                    }

                    if (file.ContentLength > limitFileSize)
                    {
                        throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                    }

                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        string guid = Guid.NewGuid().ToString("N");
                        string fileName = guid + Path.GetExtension(file.FileName);
                        string thumbName = string.Format("{0}_thumb{1}", guid, Path.GetExtension(file.FileName));

                        string absolutePath = fullPath + "/" + fileName;
                        string thumbAbsolutePath = fullPath + "/" + thumbName;
                        relativePath = filePath + "/" + thumbName;

                        file.SaveAs(absolutePath);
                        ImageHelper.GetReducedImage(150, 150, absolutePath, thumbAbsolutePath);
                        File.Delete(absolutePath);

                        var staffAvatar = WxappDao.QueryStaffAvatar(id);
                        if (staffAvatar == null)
                        {
                            ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
                            {
                                Picture = relativePath,
                                UseWxAvatar = false,
                                StaffId = id
                            };
                            WxappDao.AddOrUpdateStaffAvatar(avatar);
                        }
                        else
                        {
                            staffAvatar.Picture = relativePath;
                            staffAvatar.UseWxAvatar = false;
                            WxappDao.AddOrUpdateStaffAvatar(staffAvatar);
                        }
                    }
                }
                return new Response
                {
                    Result = relativePath
                };
            }
            else
            {
                throw new Exception("上传失败，未接收到请求文件！");
            }
        }

        /// <summary>
        /// 使用微信头像
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public Response ZXKH_UseWxAvatar(ESSChannelStaffAvatar avatar)
        {
            var staffAvatar = WxappDao.QueryStaffAvatar(avatar.StaffId);
            if (staffAvatar == null)
            {
                avatar.UseWxAvatar = true;
                WxappDao.AddOrUpdateStaffAvatar(avatar);
            }
            else
            {
                staffAvatar.UseWxAvatar = true;
                staffAvatar.Picture = avatar.Picture;
                avatar = null;
                WxappDao.AddOrUpdateStaffAvatar(staffAvatar);
            }
            return new Response
            {
                Result = 1
            };
        }

        public Response ZXKH_QueryUseAvatar(int id)
        {
            var customer = WxappDao.QueryCustomer(id);
            var staffAvatar = WxappDao.QueryStaffAvatar(id);

            return new Response
            {
                Result = new
                {
                    avatarUrl = staffAvatar.Picture,
                    nickName = customer.KHNAME
                }
            };
        }



        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Response ZXKH_QueryUserInfo(int uid)
        {
            var result = WxappDao.QueryUserInfo(uid);

            return new Response
            {
                Result = result
            };
        }


        public Response ZXKH_GetOpenId(string code)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, code);

            WxPayloadBO wxFirst = null;
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string msg = response.Content.ReadAsStringAsync().Result;
                wxFirst = JsonConvert.DeserializeObject<WxPayloadBO>(msg);
                wxFirst.session_key = "";
            }
            return new Response
            {
                Result = wxFirst
            };
        }


        public Response ZXKH_GetAccessToken()
        {
            var token = Common.WxService.WxHelper.GetAccessToken(AppId, Secret);
            return new Response
            {
                Result = token
            };
        }

        public async Task<IList<CustomerServiceMessageVO>> ZXKH_QueryCustomerMsg(string wxopenid, string openid, int page, int limit)
        {
            var result = WxappDao.ZXKH_QueryCustomerMessage(wxopenid, openid, page, limit);

            IList<CustomerServiceMessageVO> vos = AutoMapper.Mapper.Map<List<CustomerServiceMessageVO>>(result);
            foreach (var item in vos)
            {
                if (item.MsgType == TENCENT_MSG_TYPE_CARD)
                {
                    string goodsId = item.PagePath.Substring(item.PagePath.IndexOf("=") + 1);
                    item.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
                }

                if (int.TryParse(item.FromUserName, out int userId))
                {
                    var user = WxappDao.QueryUserInfo(userId);
                    //item.FromUserName = user["KHNAME"];
                    item.ThumbUrl = user["PICTURE"];
                }
            }
            return await Task.FromResult(vos);
        }
        public async Task<Response> ZXKH_QueryGroupMsg(string wxopenid, int page, int limit)
        {
            var result = WxappDao.ZXKH_QueryGroupMsg(wxopenid, page, limit);

            //IList<CustomerServiceMessageVO> vos = AutoMapper.Mapper.Map<List<CustomerServiceMessageVO>>(result);
            //foreach (var item in vos)
            //{
            //    if (item.MsgType == TENCENT_MSG_TYPE_CARD)
            //    {
            //        string goodsId = item.PagePath.Substring(item.PagePath.IndexOf("=") + 1);
            //        item.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
            //    }

            //    if (int.TryParse(item.FromUserName, out int userId))
            //    {
            //        var user = WxappDao.QueryUserInfo(userId);
            //        item.FromUserName = user["KHNAME"];
            //        item.ThumbUrl = user["PICTURE"];
            //    }
            //}
            return await Task.FromResult(new Response { Result = result });
        }

        public Task<CustomerServiceMessageVO> ZXKH_QueryCustomerLastMsg(string wxopenid)
        {
            var result = WxappDao.ZXKH_QueryCustomerLastMessage(wxopenid);

            var vo = AutoMapper.Mapper.Map<CustomerServiceMessageVO>(result);
            if (vo != null)
            {
                if (int.TryParse(vo.FromUserName, out int userId))
                {
                    var user = WxappDao.ZXKH_QueryUserInfo(userId);
                    vo.FromUserName = user["KHNAME"];
                    vo.ThumbUrl = user["PICTURE"];
                }

                if (vo.MsgType == TENCENT_MSG_TYPE_CARD)
                {
                    string goodsId = vo.PagePath.Substring(vo.PagePath.IndexOf("=") + 1);
                    vo.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
                }
            }
            return Task.FromResult(vo);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="fromUserName"></param>
        public void ZXKH_KHDel(string fromUserName)
        {
            WxappDao.ZXKH_KHDel(fromUserName);
        }

        public Task<Response> ZXKH_UpdateCustomerServiceStaff(int id, WxPayloadVO wxPayload)
        {
            WxappDao.ZXKH_UpdateUserName(id, wxPayload.NickName);
            var staffAvatar = WxappDao.ZXKH_QueryStaffAvatar(id);
            staffAvatar.Picture = wxPayload.AvatarUrl;

            var result = new Response
            {
                Result = 1
            };
            return Task.FromResult(result);
        }

        public async Task<Response> ZXKH_QueryCustomers(string fmobile)
        {
            var result = WxappDao.ZXKH_QueryCustomers();

            foreach (var item in result)
            {
                var openid = (string)item["XCXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["createtime"]).AddHours(-8)).TotalMinutes;

                var result1 = WxappDao.ZXKH_QueryMyCustomerFid(fmobile);
                foreach (var item1 in result1)
                {
                    if (item1["CustomerID"] == item["FID"])
                    {
                        item["identity"] = "客户";
                    }
                }

                var staff = WxappDao.ZXKH_QueryMyStaffFid(fmobile);
                foreach (var item2 in staff)
                {
                    if (item2["StaffID"] == item["FID"])
                    {
                        item["identity"] = "客服";
                    }
                }
            }

            //result = result.Where(x => x["diffMinutes"] <= 48 * 60).ToList();

            return new Response
            {
                Result = result
            };
        }
        public async Task<Response> ZXKH_QueryMyCustomers(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryMyCustomers(fmobile);
            int index = 1;
            foreach (var item in results)
            {
                var openid = (string)item["XCXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                if (item["CreateTime"]!=null)
                {
                    item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["CreateTime"]).AddHours(-8)).TotalMinutes;
                    //item["khname"] = "我的客户" + index.ToString();
                }
                index++;
            }

            return new Response
            {
                Result = results
            };
        }
        public async Task<Response> ZXKH_QueryMyStaff(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryMyStaff(fmobile);

            foreach (var item in results)
            {
                var openid = (string)item["XCXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                if (item["CreateTime"] != null)
                {
                    item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["CreateTime"]).AddHours(-8)).TotalMinutes;
                    //item["khname"] = "我的客服";
                }
            }

            return new Response
            {
                Result = results
            };
        }
        
        public async Task<Response> ZXKH_QueryMyMessage(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryMyMessage(fmobile);

            foreach (var item in results)
            {
                var openid = (string)item["XCXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["CreateTime"]).AddHours(-8)).TotalMinutes;
                var result = WxappDao.ZXKH_QueryMyCustomerFid(fmobile);
                foreach (var item1 in result)
                {
                    if (item1["CustomerID"] == item["FID"])
                    {
                        item["identity"] = "客户";
                    }
                }

                var staff = WxappDao.ZXKH_QueryMyStaffFid(fmobile);
                foreach (var item2 in staff)
                {
                    if (item2["StaffID"] == item["FID"])
                    {
                        item["identity"] = "客服";
                    }
                }
            }

            return new Response
            {
                Result = results
            };
        }

        public async Task<Response> ZXKH_Friends(string fmobile)
        {
            var results = WxappDao.ZXKH_Friends(fmobile);

            //foreach (var item in results)
            //{
            //    var openid = (string)item["FWXOPENID"];
            //    var count = RedisHelper.StringGet(openid);
            //    item["count"] = count == "0" ? "" : count;
            //    item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["CreateTime"]).AddHours(-8)).TotalMinutes;
            //}

            return new Response
            {
                Result = results
            };
        }
        public async Task<Response> ZXKH_QueryApply(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryApply(fmobile);

            return new Response
            {
                Result = results
            };
        }
        public async Task<Response> ZXKH_GroupList(string fmobile)
        {
            var results = WxappDao.ZXKH_GroupList(fmobile);
            foreach (var item in results)
            {
                var groupno = item["GroupNo"];  //定义群组的id

                var groupname = item["GroupName"];   //定义群组的名称
                if (groupname == null)
                {
                    var userName = WxappDao.ZXKH_GroupName(groupno);  //定义群组下面成员的名称
                    for (int i = 0; i < userName.Count; i++)   //如果群组名称没有则用成员拼凑
                    {
                        if (i == 0)
                        {
                            item["GroupName"] = userName[i];
                        }
                        else
                        {
                            item["GroupName"] += "、" + userName[i];
                            if (i > 1)
                            {
                                item["GroupName"] += "...";
                                break;
                            }
                        }
                    }
                }
                //获取群最后一条群成员发送的消息
                var groupmessage = WxappDao.ZXKH_GroupBy(groupno);
                if (groupmessage.Count == 1)
                {
                    item["content"] = groupmessage[0][0];
                    item["createtime"] = groupmessage[0][1];
                }

                //如果图片为空则添加
                if (item["GroupImgBase64"] == null || item["GroupImgBase64"] == "")
                {
                    var bb = WxappDao.ZXKH_GroupImg(groupno);
                    string[] path = new string[bb.Count];
                    for (int i = 0; i < bb.Count; i++)
                    {
                        path[i] = bb[i];
                    }
                    JiuGongDiagram jiuGong = new JiuGongDiagram();
                    var bitmap = jiuGong.Synthetic(path, true);
                    item["GroupImgBase64"] = "data:image/jpeg;base64," + ZXKH_ToBase64(bitmap);
                    WxappDao.ZXKH_GroupImgBase64(groupno, item["GroupImgBase64"]);
                    ////可以保存到本地或者上传到文件服务器
                    //bitmap.Save(@"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\4.jpg", System.DrawingCore.Imaging.ImageFormat.Jpeg);

                    //System.Drawing.Image img = System.Drawing.Image.FromHbitmap(bitmap.GetHbitmap());
                    //item["picture"] = @"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\1.jpg";
                    bitmap.Dispose();
                }
            };
            return new Response
            {
                Result = results
            };
        }

        public async Task<Response> ZXKH_ALLGroupList()
        {
            var results = WxappDao.ZXKH_ALLGroupList();
            foreach (var item in results)
            {
                var groupno = item["GroupNo"];  //定义群组的id

                var groupname = item["GroupName"];   //定义群组的名称
                if (groupname == null)
                {
                    var userName = WxappDao.ZXKH_GroupName(groupno);  //定义群组下面成员的名称
                    for (int i = 0; i < userName.Count; i++)   //如果群组名称没有则用成员拼凑
                    {
                        if (i == 0)
                        {
                            item["GroupName"] = userName[i];
                        }
                        else
                        {
                            item["GroupName"] += "、" + userName[i];
                            if (i > 1)
                            {
                                item["GroupName"] += "...";
                                break;
                            }
                        }
                    }
                }
                //获取群最后一条群成员发送的消息
                var groupmessage = WxappDao.ZXKH_GroupBy(groupno);
                if (groupmessage.Count == 1)
                {
                    item["content"] = groupmessage[0][0];
                    item["createtime"] = groupmessage[0][1];
                }

                //如果图片为空则添加
                if (item["GroupImgBase64"] == null || item["GroupImgBase64"] == "")
                {
                    var bb = WxappDao.ZXKH_GroupImg(groupno);
                    string[] path = new string[bb.Count];
                    for (int i = 0; i < bb.Count; i++)
                    {
                        path[i] = bb[i];
                    }
                    JiuGongDiagram jiuGong = new JiuGongDiagram();
                    var bitmap = jiuGong.Synthetic(path, true);
                    item["GroupImgBase64"] = "data:image/jpeg;base64," + ZXKH_ToBase64(bitmap);
                    WxappDao.ZXKH_GroupImgBase64(groupno, item["GroupImgBase64"]);
                    ////可以保存到本地或者上传到文件服务器
                    //bitmap.Save(@"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\4.jpg", System.DrawingCore.Imaging.ImageFormat.Jpeg);

                    //System.Drawing.Image img = System.Drawing.Image.FromHbitmap(bitmap.GetHbitmap());
                    //item["picture"] = @"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\1.jpg";
                    bitmap.Dispose();
                }
            };
            return new Response
            {
                Result = results
            };
        }
        

        /// <summary>
        /// 通过手机号码查询此人
        /// </summary>
        /// <param name="FMOBILE"></param>
        /// <returns></returns>
        public async Task<Response> ZXKH_QueryUser(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryUser(fmobile);

            return new Response
            {
                Result = results
            };
        }
        /// <summary>
        /// 查找好友关系
        /// </summary>
        /// <param name="FMOBILE"></param>
        /// <returns></returns>
        public async Task<Response> ZXKH_QueryFriend(long user, long userfriend)
        {
            var results =WxappDao.ZXKH_QueryFriend(user, userfriend);

            return new Response
            {
                Result = results
            };
        }
        /// <summary>
        /// 发送好友申请
        /// </summary>
        /// <param name="user"></param>
        /// /// <param name="userfriend"></param>
        /// <returns></returns>
        public void ZXKH_SendApply(long user, long userfriend)
        {
            var hh = WxappDao.ZXKH_QueryFriendApply(user, userfriend);
            if (hh.Count == 0)
            {
                WxappDao.ZXKH_SendApply(user, userfriend);
            }
        }
        /// <summary>
        /// 添加好友关系
        /// </summary>
        /// <param name="fmobile"></param>
        /// <returns></returns>
        public async void ZXKH_Friendadd(long user, long userfriend)
        {
            WxappDao.ZXKH_Friendadd(user, userfriend);
        }


        /// <summary>
        /// 创建群组
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public async void ZXKH_GroupCreat(string array)
        {
            var groupNo = ZXKH_GenerateGuid().ToString();
            var list1 = array.Replace("[", null);
            list1 = list1.Replace("]", null);
            var list = list1.Split(',');
            for (int i = list.Length - 1; i > -1; i--)
            {
                if (i == list.Length - 1)
                {
                    WxappDao.ZXKH_GroupCreat(long.Parse(list[i]), groupNo);
                }
                WxappDao.ZXKH_GroupandUser(long.Parse(list[i]), groupNo);
            }
            //for (int i = 0; i < list.Length; i++)
            //{
            //    WxappDao.GroupandUser(long.Parse(list[i]), groupNo);
            //    if (i == list.Length - 1)
            //    {
            //        WxappDao.GroupCreat(long.Parse(list[i]), groupNo);
            //    }
            //}
        }
        public async void ZXKH_GroupRemove(string array, string groupno)
        {
            var list1 = array.Replace("[", null);
            list1 = list1.Replace("]", null);
            var list = list1.Split(',');
            for (int i = 0; i < list.Length; i++)
            {
                WxappDao.ZXKH_GroupRemove(list[i], groupno);
            }

            var bb = WxappDao.ZXKH_GroupImg(groupno);
            string[] path = new string[bb.Count];
            for (int i = 0; i < bb.Count; i++)
            {
                path[i] = bb[i];
            }
            JiuGongDiagram jiuGong = new JiuGongDiagram();
            var bitmap = jiuGong.Synthetic(path, true);
            var GroupImgBase64 = "data:image/jpeg;base64," + ZXKH_ToBase64(bitmap);
            WxappDao.ZXKH_GroupImgBase64(groupno, GroupImgBase64);
            bitmap.Dispose();
        }

        public async void ZXKH_GroupAdd(string array, string groupno)
        {
            var list1 = array.Replace("[", null);
            list1 = list1.Replace("]", null);
            var list = list1.Split(',');
            for (int i = 0; i < list.Length; i++)
            {
                WxappDao.ZXKH_GroupAdd(list[i], groupno);
            }

            var bb = WxappDao.ZXKH_GroupImg(groupno);
            string[] path = new string[bb.Count];
            for (int i = 0; i < bb.Count; i++)
            {
                path[i] = bb[i];
            }
            JiuGongDiagram jiuGong = new JiuGongDiagram();
            var bitmap = jiuGong.Synthetic(path, true);
            var GroupImgBase64 = "data:image/jpeg;base64," + ZXKH_ToBase64(bitmap);
            WxappDao.ZXKH_GroupImgBase64(groupno, GroupImgBase64);
            bitmap.Dispose();
        }
        public void ZXKH_GroupVoid(string groupno)
        {
            WxappDao.ZXKH_GroupVoid(groupno);
        }



        public async Task<Response> ZXKH_GroupManagement(string groupno)
        {
            var results = WxappDao.ZXKH_GroupManagement(groupno);


            return new Response
            {
                Result = results
            };
        }
        public async Task<Response> ZXKH_GroupInformation(string groupno)
        {
            var results = WxappDao.ZXKH_GroupInformation(groupno);


            return new Response
            {
                Result = results
            };
        }

        public async Task<bool> ZXKH_EditGroupName(string groupname, string groupno)
        {
            WxappDao.ZXKH_EditGroupName(groupname, groupno);
            return true;
        }
        public dynamic ZXKH_BoolGroupUse(string useropenid, string groupno)
        {
            dynamic results =WxappDao.ZXKH_BoolGroupUse(useropenid, groupno);
            return results;
        }
        

        public Response ZXKH_UserWebSocket(string userID,string connectionID)
        {
            if (WxappDao.ZXKH_QueryUserWebSocket(userID) == null)
            {
                WxappDao.ZXKH_AddUserWebSocket(userID, connectionID);
                return new Response {
                    Result = 1
                };
            }
            else
            {
                WxappDao.ZXKH_EditUserWebSocket(userID, connectionID);
                return new Response
                {
                    Result = 2
                };
            }
        }
        public dynamic ZXKH_SendWebSocket(string userID)
        {
            dynamic result = WxappDao.ZXKH_QueryUserWebSocket(userID);
            return result;
        }
        


        public async Task<Response> ZXKH_QueryKhCustomer(string fmobile)
        {
            var results = WxappDao.ZXKH_QueryKhCustomer(fmobile);

            foreach (var item in results)
            {
                var openid = (string)item["FWXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["CreateTime"]).AddHours(-8)).TotalMinutes;
            }

            return new Response
            {
                Result = results
            };
        }

        /// <summary>
        /// 判断用户是否与客服有关联
        /// </summary>
        /// <param name="FWXOPENID"></param>
        /// <returns></returns>
        public bool ZXKH_Staff_Customers(string FWXOPENID)
        {
            var result = WxappDao.ZXKH_Staff_Customers(FWXOPENID);
            if (result.Count == 0)
            {
                return true;
            }
            else
            {
                long staffid = result[0]["StaffID"];
                if (staffid == 0)
                {
                    WxappDao.ZXKH_Delete_Customers(FWXOPENID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 添加客户与客服的关联
        /// </summary>
        /// <param name="FWXOPENID"></param>
        public void ZXKH_AddShip(string FWXOPENID)//需要改动
        {
            var result = WxappDao.ZXKH_SelectStaff();
            if (result.Count != 0)               //判断管理人员是否为空
            {
                int fpkid = (int)result[result.Count - 1]["FID"];  //选取最后一个管理人id
                WxappDao.ZXKH_AddShip(FWXOPENID, fpkid, ZXKH_ConvertDateTimeInt(DateTime.Now));  //添加关系

                ////记录当用户发送消息给客服时当前时间
                //CustomerServiceMessage WxXmlModels = new CustomerServiceMessage();
                //WxXmlModels.MsgId = MsgId;
                //WxXmlModels.ToUserName = FWXOPENID;
                //WxXmlModels.FromUserName = WxappDao.SelectMSGLast(FWXOPENID)[0]["FWXOPENID"];
                //WxXmlModels.MsgType = MsgType;
                //WxXmlModels.CreateTime = ConvertDateTimeInt(DateTime.Now);
                //WxXmlModels.Content = "系统时间";
                //WxappDao.savemessage(WxXmlModels);
            }
            else
            {
                WxappDao.ZXKH_AddShip(FWXOPENID, 0, ZXKH_ConvertDateTimeInt(DateTime.Now));
            }

        }


        //string fOPENID;
        /// <summary>
        /// 修改用户关系
        /// </summary>
        /// <param name="OPENID"></param>
        public void ZXKH_EditShip(string OPENID)//需要改动
        {

            var resultFirst = WxappDao.ZXKH_SelectMSGFirst(OPENID);
            var resultLast = WxappDao.ZXKH_SelectMSGLast(OPENID);
            var time = resultFirst[0]["createtime"].ToString();//获取查询到的第一个数据，一般都是一个，除非你是客服且匹配到了自己
            var time1 = resultLast[0]["createtime"].ToString();//获取到最后一条数据时间
            var FPKIDs = resultFirst[0]["FPKID"];
            DateTime dateTime = ZXKH_ConvertToDateTime(time); //时间戳转换为日期格式
            ZXKH_WriteTxt(dateTime.ToString());
            DateTime dt = System.DateTime.Now;  //系统时间
            TimeSpan d3 = dt.Subtract(dateTime);  //客户匹配客服消息的时间
            long hh = (int)d3.Days * 3600 + (int)d3.Hours * 60 + (int)d3.Minutes;
            ZXKH_WriteTxt(hh.ToString() + "分钟");
            if (hh > 10)           //如果第一次消息时间大于10十分钟
            {
                if (time != time1)   //如果客服进行了回复
                {
                    ZXKH_WriteTxt("客服已回复，无需更改！");
                }
                else
                {
                    var result1 = WxappDao.ZXKH_SelectStaff();
                    int fpkid; //管理人id
                    ArrayList array = new ArrayList();
                    ///将全部管理人添加到ArrayList数组中
                    for (int i = 0; i < result1.Count; i++)
                    {
                        fpkid = (int)result1[i]["FID"];
                        array.Add(fpkid);
                    }
                    //截取前面部分活跃的管理人
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i].ToString() == FPKIDs.ToString())
                        {
                            array.RemoveRange(i, array.Count - i);
                            break;
                        }
                    }
                    //如果前面没有人则不用修改
                    if (array.Count != 0)
                    {
                        var jj = (int)array[array.Count - 1];//找到当前客服最后一人
                        WxappDao.ZXKH_EditShip(OPENID, jj);
                    }
                    else
                    {
                        ZXKH_WriteTxt("只有这个客服在服务，无需修改");
                    }
                }
            }
            else
            {
                ZXKH_WriteTxt("时间未到，无需修改");
            }
        }

        /// <summary>
        /// 查询对应客服openid
        /// </summary>
        /// <param name="OPENID"></param>
        public dynamic ZXKH_SelectShip(string OPENID)//需要改动
        {
            dynamic staff = WxappDao.ZXKH_SelectShip(OPENID);
            return staff;
        }


        /// <summary>
        /// Unix时间戳转DateTime
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static DateTime ZXKH_ConvertToDateTime(string timestamp)
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            if (timestamp.Length == 10)        //精确到秒
            {
                time = startTime.AddSeconds(double.Parse(timestamp));
            }
            else if (timestamp.Length == 13)   //精确到毫秒
            {
                time = startTime.AddMilliseconds(double.Parse(timestamp));
            }
            return time;
        }



        public static Response ZXKH_savemessage(CustomerServiceMessage WxXmlModels)
        {

            WxappDao.ZXKH_savemessage(WxXmlModels);

            return new Response
            {
                Result = 1
            };
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="content"></param>
        public void ZXKH_SendText(string openid, string content,string fromuser)
        {
            string access_token = ZXKH_IsExistAccess_Token();//微信认证
            string url = ZXKH_GetKFSend(access_token);
            JObject data = new JObject();
            data.Add("touser", openid);
            data.Add("msgtype", "text");
            data.Add("text", JObject.FromObject(new
            {
                content = "来自小程序" + Environment.NewLine + fromuser + ":" + content + Environment.NewLine + "点此进入=>" + "<a data-miniprogram-appid='wx05551978a9c3ba4b' data-miniprogram-path='pages/chatcontact/main' href='http://www.qq.com'>数字鱼小程序</a>"
            }));
            NetHelper.Post(url, data.ToString());
        }
        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="media_id"></param>
        public void ZXKH_SendImage(string openid, string fromuser)
        {
            //string access_token = ZXKH_IsExistAccess_Token();//微信认证
            //string url = ZXKH_GetKFSend(access_token);
            //JObject data = new JObject();
            //data.Add("touser", openid);
            //data.Add("msgtype", "image");
            //data.Add("image", JObject.FromObject(new
            //{
            //    media_id = media_id
            //}));
            //NetHelper.Post(url, data.ToString());
            string access_token = ZXKH_IsExistAccess_Token();//微信认证
            string url = ZXKH_GetKFSend(access_token);
            JObject data = new JObject();
            data.Add("touser", openid);
            data.Add("msgtype", "text");
            data.Add("text", JObject.FromObject(new
            {
                content = "来自小程序" + Environment.NewLine + fromuser + ":" + "[图片]" + Environment.NewLine + "请进入小程序查看=》" + "<a data-miniprogram-appid='wx05551978a9c3ba4b' data-miniprogram-path='pages/chatcontact/main' href='http://www.qq.com'>数字鱼小程序</a>"
            }));
            NetHelper.Post(url, data.ToString());

        }
        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="media_id"></param>
        public void ZXKH_SendVoice(string openid, string fromuser)
        {
            //string access_token = ZXKH_IsExistAccess_Token();//微信认证
            //string url = ZXKH_GetKFSend(access_token);
            //JObject data = new JObject();
            //data.Add("touser", openid);
            //data.Add("msgtype", "voice");
            //data.Add("voice", JObject.FromObject(new
            //{
            //    media_id = media_id
            //}));
            //NetHelper.Post(url, data.ToString());
            string access_token = ZXKH_IsExistAccess_Token();//微信认证
            string url = ZXKH_GetKFSend(access_token);
            JObject data = new JObject();
            data.Add("touser", openid);
            data.Add("msgtype", "text");
            data.Add("text", JObject.FromObject(new
            {
                content = "来自小程序" + Environment.NewLine + fromuser + ":" + "[语音]"+ Environment.NewLine+"请进入小程序查看=》" + "<a data-miniprogram-appid='wx05551978a9c3ba4b' data-miniprogram-path='pages/chatcontact/main' href='http://www.qq.com'>数字鱼小程序</a>"
            }));
            NetHelper.Post(url, data.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="media_id"></param>
        public dynamic ZXKH_OpenidToName(string XCXOPENID)
        {
            return WxappDao.ZXKH_OpenidToName(XCXOPENID);
        }

        /// <summary>
        /// 通用，修改appid和secret就行,xml保存
        /// </summary>
        /// <returns></returns>
        public static Access_token ZXKH_GetAccess_Token()
        {
            string strUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + gzhAppId + "&secret=" + gzhSecret;
            Access_token mode = new Access_token();
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                //Response.Write(content);  
                //在这里对Access_token 赋值  
                Access_token token = new Access_token();
                token = JsonHelper.ParseFromJson<Access_token>(content);
                mode.access_token = token.access_token;
                mode.expires_in = token.expires_in;
            }
            return mode;
        }
        /// <summary>  
        /// 根据当前日期 判断Access_Token 是否超期  如果超期返回新的Access_Token   否则返回之前的Access_Token  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static string ZXKH_IsExistAccess_Token()
        {

            string Token = string.Empty;
            DateTime YouXRQ;
            // 读取XML文件中的数据，并显示出来 ，注意文件路径  
            //string filepath = HttpContext.Current.Server.MapPath("/XMLFile.xml");
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "XMLFile.xml";
            //string filepath = Path.GetFullPath("../../XMLFile.xml");
            StreamReader str = new StreamReader(filepath, System.Text.Encoding.UTF8); 
            XmlDocument xml = new XmlDocument();
            xml.Load(str);
            str.Close();
            str.Dispose();
            Token = xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText;
            YouXRQ = Convert.ToDateTime(xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText);

            if (DateTime.Now > YouXRQ)
            {
                DateTime _youxrq = DateTime.Now; 
                Access_token mode = ZXKH_GetAccess_Token();
                xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText = mode.access_token;
                _youxrq = _youxrq.AddSeconds(int.Parse(mode.expires_in));
                xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText = _youxrq.ToString();
                xml.Save(filepath);
                Token = mode.access_token;
            }
            return Token;
        }
        /// <summary>
        /// 获取url
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public string ZXKH_GetKFSend(string access_token)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token);
            return url;
        }

        /// <summary>
        /// 通过mediaId获取图片
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public static string ZXKH_MediaIdRequestImg(string mediaId)
        {
            var token = ZXKH_IsExistAccess_Token();
            string url = String.Format("https://api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            var filePath = ZXKH_HttpDownloadImg(url);
            var name = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            return "ZXKF_UpLoad/" + name;
        }

        /// <summary>
        /// 通过mediaId获取音频
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public static string ZXKH_MediaIdRequestData(string mediaId)
        {
            var token = ZXKH_IsExistAccess_Token();
            string url = String.Format("https://api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            var filePath = ZXKH_HttpDownloadFile(url);
            //var newfile = filePath.Substring(0, filePath.LastIndexOf("."));
            //var newFilePath = newfile + ".mp3".Replace(".amr", ".mp3");
            var newFilePath = filePath.Replace(".amr", ".mp3");
            ZXKH_FormatConversion(filePath, "amr", newFilePath, "mp3");
            var name = newFilePath.Substring(newFilePath.LastIndexOf("\\") + 1);
            return "ZXKF_UpLoad/" + name;
        }

        /// <summary>
        /// 格式转化
        /// </summary>
        /// <param name="inputFile">源文件路径</param>
        /// <param name="inputFormat">源文件格式</param>
        /// <param name="outFile">转化后文件路径</param>
        /// <param name="outFormat">转化后文件格式</param>
        public static void ZXKH_FormatConversion(string inputFile, string inputFormat, string outFile, string outFormat, int audioSampleRate = 44100)
        {
            try
            {
                new FFMpegConverter().ConvertMedia(inputFile, inputFormat, outFile, outFormat, new ConvertSettings { AudioSampleRate = audioSampleRate });
            }
            catch (Exception ex)
            {
                throw ex;
                // ignored
            }
        }
        ///
        /// Http下载文件：下载指定文件存储到特定目录
        ///
        public static string ZXKH_HttpDownloadImg(string remoteUrl)
        {
            string LogPath = HttpContext.Current.Server.MapPath("/ZXKF_UpLoad/");
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            // 设置参数
            HttpWebRequest request = WebRequest.Create(remoteUrl) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "ZXKF_UpLoad\\" + DateTime.Now.ToFileTime() + ".jpg";

            //创建本地文件写入流
            Stream stream = new FileStream(localPath, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return localPath;
        }

        ///
        /// Http下载文件：下载指定文件存储到特定目录
        ///
        public static string ZXKH_HttpDownloadFile(string remoteUrl)
        {
            string LogPath = HttpContext.Current.Server.MapPath("/ZXKF_UpLoad/");
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            // 设置参数
            HttpWebRequest request = WebRequest.Create(remoteUrl) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "ZXKF_UpLoad\\" + DateTime.Now.ToFileTime() + ".amr";
            //string localPath = "D:\\\\keli\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".amr";
            //string localPath = Path.GetFullPath("../../downloadaudio/" + DateTime.Now.ToFileTime() + ".amr");

            //创建本地文件写入流
            Stream stream = new FileStream(localPath, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return localPath;
        }
        public static string ZXKH_HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>
        /// 记录bug，以便调试
        /// </summary>
        public static bool ZXKH_WriteTxt(string str)
        {
            try
            {
                string LogPath = HttpContext.Current.Server.MapPath("/Logs/");
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                FileStream FileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("/Logs//xiejun_" + DateTime.Now.ToLongDateString() + "_.txt"), FileMode.Append);
                StreamWriter StreamWriter = new StreamWriter(FileStream);
                //开始写入
                StreamWriter.WriteLine(str);
                //清空缓冲区
                StreamWriter.Flush();
                //关闭流
                StreamWriter.Close();
                FileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 客户
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Response ZXKH_BindCustomers(string openid)
        {
            
            HttpClient client = new HttpClient();
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", gzhAppId, gzhSecret);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            ZXKH_WriteTxt(msg);
            //HttpClient client = new HttpClient();
            url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", data.access_token, openid);
            HttpResponseMessage responses = client.GetAsync(url).Result;
            responses.EnsureSuccessStatusCode();

            string msgs = responses.Content.ReadAsStringAsync().Result;
            dynamic datas = JsonConvert.DeserializeObject<dynamic>(msgs);


            //查询手机号是否已注册 
            ISession session = NHSessionProvider.SessionFactory.OpenSession();
            var sql = string.Format(@"SELECT  A.FID,a.FWXOPENID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID
                   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON(TTL.FTYPEID = L.FCHANNELTYPEID AND TTL.FLOCALEID = 2052)  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID, '') <> '' AND  A.FWXOPENID = :P1");
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", openid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();

            if (staff == null)
            {
                var customer = staff;
                if (customer == null)
                {
                    ChannelStaffVO vo = new ChannelStaffVO();
                    vo.ISNEW = false;
                    vo.FCHANNELID = 27;
                    vo.FMOBILE = "18174419350";//手机号码
                    vo.FWXOPENID = openid;//openid
                    vo.FENABLE = '1';
                    vo.FTELE = "0";
                    vo.FCREATEDATE = System.DateTime.Now;
                    vo.FMODIFYDATE = System.DateTime.Now;
                    vo.FCREATORID = 0;
                    vo.FMODIFIERID = 0;
                    vo.FQQ = "0";
                    vo.ISNEW = false;
                    vo.KHNAME = datas.nickname;
                    vo.A3ID = 0;

                    var sql1 = "INSERT INTO[dbo].[T_ESS_CHANNELSTAFF]([FCHANNELID],[FMOBILE],[FTELE],[FQQ],[FWXOPENID],[FWECHAT],[FCREATORID],[FCREATEDATE],[FMODIFIERID],[FMODIFYDATE],[FENABLE],[SALT],[PASSWORD],[KHNAME],[KHSH],[KHZCDZ],[KHSHDZ],[KHTEL],[KHBANK],[KHBANKZH],[GENDER],[BIRTHDAY],[AREA],[ISNEW],[A3ID])";
                    sql1 += $"VALUES({ vo.FCHANNELID},'{ vo.FMOBILE}','{vo.FTELE}','{vo.FQQ}','{vo.FWXOPENID}',null,null,'{vo.FCREATEDATE}','{vo.FCREATORID}','{vo.FMODIFYDATE}','{vo.FMODIFIERID}','{vo.FENABLE}','{vo.FENABLE}','{vo.KHNAME}',null,null,null,null,null,null,null,null,null,null,{vo.A3ID})";
                    var o = session.CreateSQLQuery(sql1).ExecuteUpdate();


                    //  NHSessionProvider.GetCurrentSession().Flush();

                    ChannelStaffLVO staffLVO = new ChannelStaffLVO
                    {
                        FNAME = "微信注册用户",
                        FJOB = "客户",
                        FREMARK = string.Empty,
                        FROLEID = 1001
                    };


                    // ISession session1= NHSessionProvider.GetCurrentSession();
                    string sql2 = $"SELECT FID FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                    var FID = session.CreateSQLQuery(sql2).List();//执行查询
                    sql2 = $"SELECT KHNAME FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                    var KHNAME = session.CreateSQLQuery(sql2).List();//执行查询



                    sql = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_AVATAR] ([PICTURE],[USEWXAVATAR],[STAFFID]) VALUES('{datas.headimgurl}',{0},{FID[0]})");
                    session.CreateSQLQuery(sql).ExecuteUpdate();


                    var sql3 = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_L]([FPKID],[FID],[FLOCALEID],[FNAME] ,[FJOB],[FREMARK],[FROLEID]) VALUES({ FID[0]},{ FID[0]},{2052},'{KHNAME[0]}','{staffLVO.FJOB}','','{staffLVO.FROLEID}')");
                    session.CreateSQLQuery(sql3).ExecuteUpdate();



                    // StaffService.SaveAvatar(avatar);
                    //vo.ChannelStaffLVOs = staffLVO;

                    //StaffService.Save(vo);

                    //NHSessionProvider.GetCurrentSession().Flush();

                    // sql = string.Format(@"SELECT  A.FID,a.FWXOPENID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    // FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID
                    //LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID
                    //LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                    //LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON(TTL.FTYPEID = L.FCHANNELTYPEID AND TTL.FLOCALEID = 2052)  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID, '') <> '' AND  A.FWXOPENID = :P1");
                    // var customers = session
                    //            .CreateSQLQuery(sql)
                    //            .SetParameter("P1", openid)
                    //            .SetResultTransformer(new AliasToEntityMapResultTransformer())
                    //            .List<dynamic>()
                    //            .FirstOrDefault();



                    //var cc = "image/head.png";
                    //var bb = customers.FID;
                    //

                    //var a = session.CreateSQLQuery(sql).ExecuteUpdate();

                }
                else
                {
                    //customer.FMOBILE = phoneNumber;
                }
                // NHSessionProvider.GetCurrentSession().Flush();

                //staff = StaffService.QueryWxappUserByPhoneNumber("456415");
            }
            // var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);



            //var user = new
            //{
            //    userId = (int)staff["FID"],
            //    userName = (string)staff["FNAME"],
            //    channelName = (string)staff["CHANNELNAME"],
            //    channelCode = (string)staff["FCHANNELCODE"],
            //    channelId = (int)staff["FCHANNELID"],
            //    customerId = (int)staff["FCUSTOMERID"],
            //    channelTypeId = (int)staff["FCHANNELTYPEID"],
            //    channelTypeName = (string)staff["FCHANNELTYPENAME"],
            //    avatarUrl = (string)staff["PICTURE"],
            //    nickName = (string)staff["KHNAME"],
            //    phoneNumber = "5412",
            //};

            return new Response
            {
                Result = 1

            };
        }

        /// <summary>
        /// 保存客户
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Response ZXKH_BindCustomers(string openid, string FMOBILE)
        {
            
            HttpClient client = new HttpClient();
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", gzhAppId, gzhSecret);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            ZXKH_WriteTxt(msg);
            //HttpClient client = new HttpClient();
            url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", data.access_token, openid);
            HttpResponseMessage responses = client.GetAsync(url).Result;
            responses.EnsureSuccessStatusCode();

            string msgs = responses.Content.ReadAsStringAsync().Result;
            dynamic datas = JsonConvert.DeserializeObject<dynamic>(msgs);


            //查询手机号是否已注册 
            ISession session = NHSessionProvider.SessionFactory.OpenSession();
            var sql = string.Format(@"SELECT  A.FID,a.FWXOPENID, A.ISNEW, A.A3ID, l.FNUMBER FCHANNELCODE,A.FCHANNELID,A.FMOBILE,A.SALT,A.PASSWORD,B.FNAME, B.FJOB,B.FROLEID, C.FNAME AS CHANNELNAME,L.FCUSTOMERID,FCHANNELTYPEID,TTL.FNAME FCHANNELTYPENAME,CA.PICTURE,A.KHNAME
                    FROM T_ESS_CHANNELSTAFF A LEFT JOIN T_ESS_CHANNELSTAFF_L B ON B.FID = A.FID
                   LEFT JOIN T_ESS_CHANNEL L ON L.FCHANNELID = A.FCHANNELID  INNER JOIN T_ESS_CHANNEL_L C ON A.FCHANNELID = C.FCHANNELID
                   LEFT JOIN dbo.T_ESS_CHANNELSTAFF_AVATAR CA ON CA.STAFFID = A.FID
                   LEFT JOIN T_ESS_CHANNELTYPE_L TTL ON(TTL.FTYPEID = L.FCHANNELTYPEID AND TTL.FLOCALEID = 2052)  WHERE A.FENABLE = 1 AND ISNULL(A.FWXOPENID, '') <> '' AND  A.FWXOPENID = :P1");
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", openid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();

            if (staff == null)
            {
                var customer = staff;
                if (customer == null)
                {
                    ChannelStaffVO vo = new ChannelStaffVO();
                    vo.ISNEW = false;
                    vo.FCHANNELID = 27;
                    vo.FMOBILE = FMOBILE;//手机号码
                    vo.FWXOPENID = openid;//openid
                    vo.FENABLE = '1';
                    vo.FTELE = "0";
                    vo.FCREATEDATE = System.DateTime.Now;
                    vo.FMODIFYDATE = System.DateTime.Now;
                    vo.FCREATORID = 0;
                    vo.FMODIFIERID = 0;
                    vo.FQQ = "0";
                    vo.ISNEW = false;
                    vo.KHNAME = datas.nickname;
                    vo.A3ID = 0;

                    var sql1 = "INSERT INTO[dbo].[T_ESS_CHANNELSTAFF]([FCHANNELID],[FMOBILE],[FTELE],[FQQ],[FWXOPENID],[FWECHAT],[FCREATORID],[FCREATEDATE],[FMODIFIERID],[FMODIFYDATE],[FENABLE],[SALT],[PASSWORD],[KHNAME],[KHSH],[KHZCDZ],[KHSHDZ],[KHTEL],[KHBANK],[KHBANKZH],[GENDER],[BIRTHDAY],[AREA],[ISNEW],[A3ID])";
                    sql1 += $"VALUES({ vo.FCHANNELID},'{ vo.FMOBILE}','{vo.FTELE}','{vo.FQQ}','{vo.FWXOPENID}',null,null,'{vo.FCREATEDATE}','{vo.FCREATORID}','{vo.FMODIFYDATE}','{vo.FMODIFIERID}','{vo.FENABLE}','{vo.FENABLE}','{vo.KHNAME}',null,null,null,null,null,null,null,null,null,null,{vo.A3ID})";
                    var o = session.CreateSQLQuery(sql1).ExecuteUpdate();


                    //  NHSessionProvider.GetCurrentSession().Flush();

                    ChannelStaffLVO staffLVO = new ChannelStaffLVO
                    {
                        FNAME = "微信注册用户",
                        FJOB = "客户",
                        FREMARK = string.Empty,
                        FROLEID = 1001
                    };


                    // ISession session1= NHSessionProvider.GetCurrentSession();
                    string sql2 = $"SELECT FID FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                    var FID = session.CreateSQLQuery(sql2).List();//执行查询
                    sql2 = $"SELECT KHNAME FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                    var KHNAME = session.CreateSQLQuery(sql2).List();//执行查询



                    sql = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_AVATAR] ([PICTURE],[USEWXAVATAR],[STAFFID]) VALUES('{datas.headimgurl}',{0},{FID[0]})");
                    session.CreateSQLQuery(sql).ExecuteUpdate();


                    var sql3 = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_L]([FPKID],[FID],[FLOCALEID],[FNAME] ,[FJOB],[FREMARK],[FROLEID]) VALUES({ FID[0]},{ FID[0]},{2052},'{KHNAME[0]}','{staffLVO.FJOB}','','{staffLVO.FROLEID}')");
                    session.CreateSQLQuery(sql3).ExecuteUpdate();





                }
                else
                {
                    //customer.FMOBILE = phoneNumber;
                }
                // NHSessionProvider.GetCurrentSession().Flush();

                //staff = StaffService.QueryWxappUserByPhoneNumber("456415");
            }
            // var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);



            //var user = new
            //{
            //    userId = (int)staff["FID"],
            //    userName = (string)staff["FNAME"],
            //    channelName = (string)staff["CHANNELNAME"],
            //    channelCode = (string)staff["FCHANNELCODE"],
            //    channelId = (int)staff["FCHANNELID"],
            //    customerId = (int)staff["FCUSTOMERID"],
            //    channelTypeId = (int)staff["FCHANNELTYPEID"],
            //    channelTypeName = (string)staff["FCHANNELTYPENAME"],
            //    avatarUrl = (string)staff["PICTURE"],
            //    nickName = (string)staff["KHNAME"],
            //    phoneNumber = "5412",
            //};

            return new Response
            {
                Result = 1

            };
        }

        /// <summary>
        /// 判断客户是否存在
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public bool ZXKH_count(string openid)
        {
            ISession session = NHSessionProvider.SessionFactory.OpenSession();
            var sql2 = $"SELECT KHNAME FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
            return session.CreateSQLQuery(sql2).List().Count > 0;//执行查询
        }

        /// <summary>
        /// 接收文本
        /// </summary>
        /// <param name="FromUserName"></param>
        /// <param name="ToUserName"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public string ZXKH_GetText(string FromUserName, string ToUserName, string MsgType)
        {
            string xml = "";
            switch (MsgType)
            {
                case "event":
                    xml = ZXKH_ReText(FromUserName, ToUserName, "感谢你的关注！请先绑定你的手机号码！有别于后期的处理");
                    break;
                default:
                    xml = ZXKH_ReText(FromUserName, ToUserName, "感谢你的关注！");
                    break;
            }
            return xml;
        }


        /// <summary>
        /// 回复文本
        /// </summary>
        /// <param name="FromUserName">发送给谁(openid)</param>
        /// <param name="ToUserName">来自谁(公众账号ID)</param>
        /// <param name="Content">回复类型文本</param>
        /// <returns>拼凑的XML</returns>
        public string ZXKH_ReText(string FromUserName, string ToUserName, string Content)
        {

            string XML = string.Format(@"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{3}]]></Content>
                            <FuncFlag>0</FuncFlag>
                            </xml>", FromUserName, ToUserName, ZXKH_ConvertDateTimeInt(DateTime.Now), Content);
            return XML;
        }
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ZXKH_ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 根据时间产生有序的GUID编码
        /// </summary>
        /// <returns></returns>
        public static Guid ZXKH_GenerateGuid()
        {
            byte[] guidArray = Guid.NewGuid().ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;

            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
        /// <summary>
        /// bitmap格式转化为base64格式
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private string ZXKH_ToBase64(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.DrawingCore.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            String strbaser64 = Convert.ToBase64String(arr);
            return strbaser64;
        }
        /// <summary>
        /// base64格式转换为任何格式文件保存
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type">如".mp3"</param>
        public static string ZXKH_Base64ToFile(string source, string type)
        {
            try
            {
                string LogPath = HttpContext.Current.Server.MapPath("/ZXKF_UpLoad/");
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                byte[] bytes = Convert.FromBase64String(source);
                var path = DateTime.Now.ToFileTime() + type;
                string localPath = AppDomain.CurrentDomain.BaseDirectory + "ZXKF_UpLoad\\" + path;
                using (var fs = new FileStream(localPath, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
                return "ZXKF_UpLoad/" + path;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        /// <summary>
        /// base64进制图片上传到微信公众号临时文件返回参数media_id
        /// </summary>
        /// <param name="b64"></param>
        /// <returns></returns>
        public static string ZXKH_UploadImgByB64(string b64)
        {
            //access_token 需要自己获取
            string access_token = ZXKH_IsExistAccess_Token();
            string url = $"https://api.weixin.qq.com/cgi-bin/media/upload?access_token={access_token}&type=image";
            byte[] data = Convert.FromBase64String(b64);
            var boundary = "fbce142e-4e8e-4bf3-826d-cc3cf506cccc";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "KnowledgeCenter");
            client.DefaultRequestHeaders.Remove("Expect");
            client.DefaultRequestHeaders.Remove("Connection");
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.ConnectionClose = true;
            var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
            var contentByte = new ByteArrayContent(data);
            content.Add(contentByte);
            contentByte.Headers.Remove("Content-Disposition");
            contentByte.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"media\";filename=\"{Guid.NewGuid()}.png\"" + "");
            contentByte.Headers.Remove("Content-Type");
            contentByte.Headers.TryAddWithoutValidation("Content-Type", "image/png");
            try
            {
                var result2 = client.PostAsync(url, content);
                if (result2.Result.StatusCode != HttpStatusCode.OK)
                    throw new Exception(result2.Result.Content.ReadAsStringAsync().Result);
                string jsonstr = result2.Result.Content.ReadAsStringAsync().Result;
                var dic = Newtonsoft.Json.Linq.JObject.Parse(jsonstr);
                return dic["media_id"].ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// base64进制语音上传到微信公众号临时文件返回参数media_id
        /// </summary>
        /// <param name="b64"></param>
        /// <returns></returns>
        public static string ZXKH_UploadVoiceByB64(string b64)
        {
            //access_token 需要自己获取
            string access_token = ZXKH_IsExistAccess_Token();
            string url = $"https://api.weixin.qq.com/cgi-bin/media/upload?access_token={access_token}&type=voice";
            byte[] data = Convert.FromBase64String(b64);
            var boundary = "fbce142e-4e8e-4bf3-826d-cc3cf506cccc";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "KnowledgeCenter");
            client.DefaultRequestHeaders.Remove("Expect");
            client.DefaultRequestHeaders.Remove("Connection");
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.ConnectionClose = true;
            var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
            var contentByte = new ByteArrayContent(data);
            content.Add(contentByte);
            contentByte.Headers.Remove("Content-Disposition");
            contentByte.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"media\";filename=\"{Guid.NewGuid()}.mp3\"" + "");
            contentByte.Headers.Remove("Content-Type");
            contentByte.Headers.TryAddWithoutValidation("Content-Type", "voice/mp3");
            try
            {
                var result2 = client.PostAsync(url, content);
                if (result2.Result.StatusCode != HttpStatusCode.OK)
                    throw new Exception(result2.Result.Content.ReadAsStringAsync().Result);
                string jsonstr = result2.Result.Content.ReadAsStringAsync().Result;
                var dic = Newtonsoft.Json.Linq.JObject.Parse(jsonstr);
                return dic["media_id"].ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public dynamic ZXKH_QueryPicture(string xcxOpenid)
        {
            dynamic result = WxappDao.ZXKH_QueryPicture(xcxOpenid);
            return result;
        }
        /// <summary>
        /// 用户openid获取用户fid
        /// </summary>
        /// <param name="xcxOpenid"></param>
        /// <returns></returns>
        public dynamic ZXKH_QueryFID(string xcxOpenid)
        {
            dynamic result = WxappDao.ZXKH_QueryFID(xcxOpenid);
            return result;
        }
        /// <summary>
        /// 群编号获取群用户fid然后获取connectionid
        /// </summary>
        /// <param name="xcxOpenid"></param>
        /// <returns></returns>
        public List<string> ZXKH_GroupQueryFID(string xcxOpenid)
        {
            dynamic result = WxappDao.ZXKH_GroupQueryFID(xcxOpenid);
            List<System.String> List = new List<System.String>();
            foreach (var item in result)
            {
                var fid = item["UserFID"];
                var connectionid = WxappDao.ZXKH_QueryUserWebSocket(fid.ToString());
                if (connectionid != null)
                {
                    List.Add((string)connectionid["ConnectionID"]);
                }
            }
            return List;
        }
        public Response ZXKH_wxgzh(string code)
        {
            HttpClient client = new HttpClient();
            string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", gzhAppId, gzhSecret,code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);
            return new Response
            {
                Result = data
            };
        }
        /// <summary>
        /// 微信公众号菜单手机绑定-暂时无用
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="FMOBILE"></param>
        /// <returns></returns>
        public Response ZXKH_UserInfo(string openid, string FMOBILE)
        {
            
            //查询手机号是否已注册 
            ISession session = NHSessionProvider.SessionFactory.OpenSession();
            var sql = @"SELECT dbo.T_ESS_CHANNELSTAFF.* FROM  dbo.T_ESS_CHANNELSTAFF  WHERE  FMOBILE = :P1 OR FWXOPENID = :P2";
            var staff = session
                .CreateSQLQuery(sql)
                .SetParameter("P1", FMOBILE)
                .SetParameter("P2", openid)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .Count();
            ZXKH_WriteTxt(sql);
            ZXKH_WriteTxt(staff.ToString());
            ZXKH_WriteTxt(openid);
            ZXKH_WriteTxt("测试一");
            if (staff == 0 && openid != "undefined")
            {
                
                HttpClient client = new HttpClient();
                string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", gzhAppId, gzhSecret);

                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                string msg = response.Content.ReadAsStringAsync().Result;
                dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

                ZXKH_WriteTxt(msg);
                //HttpClient client = new HttpClient();
                url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", data.access_token, openid);
                HttpResponseMessage responses = client.GetAsync(url).Result;
                responses.EnsureSuccessStatusCode();

                string msgs = responses.Content.ReadAsStringAsync().Result;
                dynamic datas = JsonConvert.DeserializeObject<dynamic>(msgs);
                // 以上openid获取用户信息

                ChannelStaffVO vo = new ChannelStaffVO();
                vo.ISNEW = false;
                vo.FCHANNELID = 27;
                vo.FMOBILE = FMOBILE;//手机号码
                vo.FWXOPENID = openid;//openid
                vo.FENABLE = '1';
                vo.FTELE = "0";
                vo.FCREATEDATE = System.DateTime.Now;
                vo.FMODIFYDATE = System.DateTime.Now;
                vo.FCREATORID = 0;
                vo.FMODIFIERID = 0;
                vo.FQQ = "0";
                vo.ISNEW = false;
                vo.KHNAME = datas.nickname;
                vo.A3ID = 0;

                var sql1 = "INSERT INTO[dbo].[T_ESS_CHANNELSTAFF]([FCHANNELID],[FMOBILE],[FTELE],[FQQ],[FWXOPENID],[FWECHAT],[FCREATORID],[FCREATEDATE],[FMODIFIERID],[FMODIFYDATE],[FENABLE],[SALT],[PASSWORD],[KHNAME],[KHSH],[KHZCDZ],[KHSHDZ],[KHTEL],[KHBANK],[KHBANKZH],[GENDER],[BIRTHDAY],[AREA],[ISNEW],[A3ID])";
                sql1 += $"VALUES({ vo.FCHANNELID},'{ vo.FMOBILE}','{vo.FTELE}','{vo.FQQ}','{vo.FWXOPENID}',null,null,'{vo.FCREATEDATE}','{vo.FCREATORID}','{vo.FMODIFYDATE}','{vo.FMODIFIERID}','{vo.FENABLE}','{vo.FENABLE}','{vo.KHNAME}',null,null,null,null,null,null,null,null,null,null,{vo.A3ID})";
                var o = session.CreateSQLQuery(sql1).ExecuteUpdate();


                //  NHSessionProvider.GetCurrentSession().Flush();

                ChannelStaffLVO staffLVO = new ChannelStaffLVO
                {
                    FNAME = "微信注册用户",
                    FJOB = "客户",
                    FREMARK = string.Empty,
                    FROLEID = 1001
                };


                // ISession session1= NHSessionProvider.GetCurrentSession();
                string sql2 = $"SELECT FID FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                var FID = session.CreateSQLQuery(sql2).List();//执行查询
                sql2 = $"SELECT KHNAME FROM  T_ESS_CHANNELSTAFF WHERE   (FWXOPENID = '{openid}')";//查询
                var KHNAME = session.CreateSQLQuery(sql2).List();//执行查询



                sql = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_AVATAR] ([PICTURE],[USEWXAVATAR],[STAFFID]) VALUES('{datas.headimgurl}',{0},{FID[0]})");
                session.CreateSQLQuery(sql).ExecuteUpdate();


                var sql3 = string.Format($"INSERT INTO [dbo].[T_ESS_CHANNELSTAFF_L]([FPKID],[FID],[FLOCALEID],[FNAME] ,[FJOB],[FREMARK],[FROLEID]) VALUES({ FID[0]},{ FID[0]},{2052},'{KHNAME[0]}','{staffLVO.FJOB}','','{staffLVO.FROLEID}')");
                session.CreateSQLQuery(sql3).ExecuteUpdate();


                if (ZXKH_Staff_Customers(openid)) //判断该用户是否有客服关联，微信公众号消息发送进行人员的第一次分配
                {
                    ZXKH_AddShip(openid);//添加
                    ZXKH_WriteTxt("添加哦");
                }

            }


            string sql10 = @"SELECT * FROM dbo.T_ESS_CHANNELSTAFF WHERE FMOBILE = :P1  AND FWXOPENID IS NOT NULL";

            var staff10 = session
                .CreateSQLQuery(sql10)
                .SetParameter("P1", FMOBILE)
                .SetResultTransformer(new AliasToEntityMapResultTransformer())
                .List<dynamic>()
                .FirstOrDefault();
            ZXKH_WriteTxt(sql10);
            ZXKH_WriteTxt("测试二");
            if (staff10 == null)
            {
                string sql11 = "update T_ESS_CHANNELSTAFF set FWXOPENID = :P2 where FMOBILE = :P1";
                session.CreateSQLQuery(sql11)
                .SetParameter("P1", FMOBILE)
                .SetParameter("P2", openid)
                .ExecuteUpdate();
            }
            return new Response
            {
                Result = 1

            };
        }
        public string ZXKH_WxgzhAuth(string url) {

            //string timestamp1 = timestamp();
            //string getNoncestr1 = getNoncestr();
            //string token = "QDG6eK";
            //string Signature1 = Signature(token, timestamp1,getNoncestr1);

            //生成tokcen
            string access_token = ZXKH_IsExistAccess_Token();
            ZXKH_WriteTxt(access_token);
            ZXKH_WriteTxt("获取的认证");
            //验证签名
            string Jsapi_Ticket = GetWeiXinJsapi_Ticket(access_token);
            JObject Jsapi_TicketJo = (JObject)JsonConvert.DeserializeObject(Jsapi_Ticket);
            #region
            string rtn = "";
            string jsapi_ticket = Jsapi_TicketJo["ticket"].ToString();
            string noncestr = CreatenNonce_str();
            long timestamp = CreatenTimestamp();
            string outstring = "";
            string JS_SDK_Result = GetSignature(jsapi_ticket, noncestr, timestamp, url, out outstring);
            //拼接json串返回前台
            rtn = "{\"appid\":\"" + ConfigurationManager.AppSettings["gzhAppId"] + "\",\"jsapi_ticket\":\"" + jsapi_ticket + "\",\"noncestr\":\"" + noncestr + "\",\"timestamp\":\"" + timestamp + "\",\"outstring\":\"" + outstring + "\",\"signature\":\"" + JS_SDK_Result.ToLower() + "\"}";
            #endregion
            return rtn;

        }
        #region 获取Jsapi_Ticket
        private static string GetWeiXinJsapi_Ticket(string accessToken)
        {
            string tokenUrl = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type={1}", accessToken, "jsapi");
            var wc = new WebClient();
            var strReturn = wc.DownloadString(tokenUrl); //取得微信返回的json数据  
            return strReturn;
        }
        #endregion
        #region 基础字符
        private static string[] strs = new string[]
                               {
                                  "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
                                  "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
                               };
        #endregion
        #region 创建随机字符串
        private static string CreatenNonce_str()
        {
            Random r = new Random();
            var sb = new StringBuilder();
            var length = strs.Length;
            for (int i = 0; i < 15; i++)
            {
                sb.Append(strs[r.Next(length - 1)]);
            }
            return sb.ToString();
        }
        #endregion
        #region  创建时间戳
        private static long CreatenTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        #endregion
        #region 签名算法
        /// <summary>
        /// 签名算法
        ///本代码来自开源微信SDK项目：https://github.com/night-king/weixinSDK
        /// </summary>
        /// <param name="jsapi_ticket">jsapi_ticket</param>
        /// <param name="noncestr">随机字符串(必须与wx.config中的nonceStr相同)</param>
        /// <param name="timestamp">时间戳(必须与wx.config中的timestamp相同)</param>
        /// <param name="url">当前网页的URL，不包含#及其后面部分(必须是调用JS接口页面的完整URL)</param>
        /// <returns></returns>
        public static string GetSignature(string jsapi_ticket, string noncestr, long timestamp, string url, out string string1)
        {
            var string1Builder = new StringBuilder();
            string1Builder.Append("jsapi_ticket=").Append(jsapi_ticket).Append("&")
                          .Append("noncestr=").Append(noncestr).Append("&")
                          .Append("timestamp=").Append(timestamp).Append("&")
                          .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
            string1 = string1Builder.ToString();
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(string1, "SHA1");
        }
        #endregion
        public string bbb()
        {
            return "以上新加";
        }
        /// <summary>
        /// pc用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<Response> pc_QueryCustomers()
        {
            var result = WxappDao.pc_QueryCustomers();

            foreach (var item in result)
            {
                var openid = (string)item["XCXOPENID"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["createtime"]).AddHours(-8)).TotalMinutes;
            }

            //result = result.Where(x => x["diffMinutes"] <= 48 * 60).ToList();

            return new Response
            {
                Result = result
            };
        }
        public async Task<Response> pc_ZXKH_ALLGroupList()
        {
            var results = WxappDao.pc_ZXKH_ALLGroupList();
            foreach (var item in results)
            {

                var openid = (string)item["xcxopenid"];
                var count = RedisHelper.StringGet(openid);
                item["count"] = count == "0" ? "" : count;
                item["diffMinutes"] = DateTime.Now.Subtract(TimeStampHelper.FromTimeStamp((long)item["createtime"]).AddHours(-8)).TotalMinutes;

                var groupno = item["xcxopenid"];  //定义群组的id

                var groupname = item["GroupName"];   //定义群组的名称
                if (groupname == null)
                {
                    var userName = WxappDao.ZXKH_GroupName(groupno);  //定义群组下面成员的名称
                    for (int i = 0; i < userName.Count; i++)   //如果群组名称没有则用成员拼凑
                    {
                        if (i == 0)
                        {
                            item["GroupName"] = userName[i];
                        }
                        else
                        {
                            item["GroupName"] += "、" + userName[i];
                            if (i > 1)
                            {
                                item["GroupName"] += "...";
                                break;
                            }
                        }
                    }
                }
                //获取群最后一条群成员发送的消息
                var groupmessage = WxappDao.ZXKH_GroupBy(groupno);
                if (groupmessage.Count == 1)
                {
                    item["content"] = groupmessage[0][0];
                    item["createtime"] = groupmessage[0][1];
                }

                //如果图片为空则添加
                if (item["picUrl"] == null || item["picUrl"] == "")
                {
                    var bb = WxappDao.ZXKH_GroupImg(groupno);
                    string[] path = new string[bb.Count];
                    for (int i = 0; i < bb.Count; i++)
                    {
                        path[i] = bb[i];
                    }
                    JiuGongDiagram jiuGong = new JiuGongDiagram();
                    var bitmap = jiuGong.Synthetic(path, true);
                    item["picUrl"] = "data:image/jpeg;base64," + ZXKH_ToBase64(bitmap);
                    WxappDao.ZXKH_GroupImgBase64(groupno, item["picUrl"]);
                    ////可以保存到本地或者上传到文件服务器
                    //bitmap.Save(@"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\4.jpg", System.DrawingCore.Imaging.ImageFormat.Jpeg);

                    //System.Drawing.Image img = System.Drawing.Image.FromHbitmap(bitmap.GetHbitmap());
                    //item["picture"] = @"C:\Users\dfish001\Desktop\小程序及API\wxapp\static\images\1.jpg";
                    bitmap.Dispose();
                }
            };
            return new Response
            {
                Result = results
            };
        }

        public string pc_kf_Lastgroup(string staffId)
        {
            return WxappDao.pc_kf_Lastgroup(staffId);
        }

        public object pc_Que(string ToGroup)
        {
            return WxappDao.pc_Que(ToGroup).FirstOrDefault();
        }

        public object pc_QueryCustomerInfo(string toUserName)
        {
            return WxappDao.pc_QueryCustomerInfo(toUserName).FirstOrDefault();
        }

        public async Task<IList<CustomerServiceMessageVO>> pc_QueryCustomerMsg(string wxopenid, int page, int limit)
        {
            //查询客户关联的客服
            var relation_KF = WxappDao.relation(wxopenid);
            //IList<dynamic> result;
            //if (relation_KF ==null )
            //{
            //    //群消息
            //   result = WxappDao.pc_ZXKH_QueryGroupMsg(wxopenid, page, limit);
            //}
            //else
            //{
                //用户查询
              var result = WxappDao.pc_QueryCustomerMessage(wxopenid, relation_KF, page, limit);
            //}
          
            IList<CustomerServiceMessageVO> vos;
            //var result = WxappDao.ZXKH_QueryCustomerMessage(wxopenid, relation_KF, page, limit);
            vos = AutoMapper.Mapper.Map<List<CustomerServiceMessageVO>>(result);
            foreach (var item in vos)
            {
                if (item.MsgType == TENCENT_MSG_TYPE_CARD)
                {
                    string goodsId = item.PagePath.Substring(item.PagePath.IndexOf("=") + 1);
                    item.Content = goodsId.Substring(0, goodsId.IndexOf("&"));
                }

                //if (int.TryParse(item.FromUserName, out int userId))
                //{
                //    var user = WxappDao.QueryUserInfo(userId);
                //    item.FromUserName = user["KHNAME"];
                //    item.ThumbUrl = user["PICTURE"];
                //}
            }
            return await Task.FromResult(vos);
        }

        public async Task<Response> pc_ZXKH_QueryGroupMsg(string wxopenid, int page, int limit)
        {
            var result = WxappDao.pc_ZXKH_QueryGroupMsg(wxopenid, page, limit);

            foreach (var item in result)
            {
                item["Identifier"] = false;
            }
            return await Task.FromResult(new Response { Result = result });
        }
        public Response pc_UploadServiceImg()
        {
            var files = HttpContext.Current.Request.Files;
            var form = HttpContext.Current.Request.Form;

            CustomerServiceMessage obj = new CustomerServiceMessage
            {
                MsgType = form["msgType"],
                FromUserName = form["fromUserName"],
                ToUserName = form["toUserName"],
                XCXFromOpenId = form["XCXFromOpenId"],
                XCXToOpenId = form["XCXToOpenId"],
                Content = "",
                AppId = "",
                CreateTime = TimeStampHelper.ToTimeStamp(DateTime.Now)
            };
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string filePath = "ZXKF_UpLoad";                   ///SERVICE_IMG_PATH + DateTime.Now.ToString("yyyyMMdd");
            if (obj.MsgType == "image")
            {
                int limitFileSize = 1024 * 1024 * 2;

                string fullPath = basePath + filePath;
                string savePath = "";

                //如果目录不存在，则创建目录
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                if (files.Count > 0)
                {
                    foreach (var key in files.AllKeys)
                    {
                        var file = files[key];
                        //校验文件类型
                        string fileExtension = Path.GetExtension(file.FileName);
                        string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                        string[] fileTypeWhiteList = new string[] { ".jpg", ".jpeg", ".png" };
                        string[] fileMimeTypeWhiteList = new string[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                        {
                            throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                        }

                        if (file.ContentLength > limitFileSize)
                        {
                            throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                        }

                        if (!string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                            savePath = filePath + "/" + fileName;
                            file.SaveAs(fullPath + "/" + fileName);

                            //var token = WxHelper.GetAccessToken(AppId, Secret);
                            //var mediaId = TencentHelper.UploadTempMedia(token, fullPath + "/" + fileName);
                            var mediaId = ZXKH_UploadImgByB64(ImageToBase64(fullPath + "/" + fileName));

                            obj.MediaId = mediaId;
                            obj.Image = new
                            {
                                media_id = mediaId
                            };
                            obj.Content = savePath;

                            //ZXKH_savemessage(obj);
                            ZXKH_SendImage(obj.ToUserName, mediaId);
                            //TencentHelper.SendCustomerMessageToUser(token, TencentHelper.MSG_TYPE_IMG, string.Empty, mediaId, obj.ToUserName);
                        }
                    }
                    return new Response
                    {
                        Result = WxappDao.SaveCustomerMessage(obj)  //savePath
                    };
                }
                else
                {
                    throw new Exception("上传失败，未接收到请求文件！");
                }
            }
            else
            {
                int limitFileSize = 1024 * 1024 * 2;

                string fullPath = basePath + filePath;
                string savePath = "";

                //如果目录不存在，则创建目录
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                if (files.Count > 0)
                {
                    foreach (var key in files.AllKeys)
                    {
                        var file = files[key];
                        //校验文件类型
                        string fileExtension = Path.GetExtension(file.FileName);
                       
                        if (!Directory.Exists(fullPath + "\\" + fileExtension.Substring(1, fileExtension.Length -1)))
                        {
                            Directory.CreateDirectory(fullPath + "\\" + fileExtension.Substring(1, fileExtension.Length - 1));
                        }
                        string fileMimeType = MimeMapping.GetMimeMapping(file.FileName);
                        string[] fileTypeWhiteList = new string[] { ".txt", ".docx", ".pptx" };
                        string[] fileMimeTypeWhiteList = new string[] { "text/plain"};
                        if (!fileTypeWhiteList.Contains(fileExtension.ToLower()) || !fileMimeTypeWhiteList.Contains(fileMimeType))
                        {
                            throw new Exception($"文件{file.FileName}是不支持的文件类型！");
                        }

                        if (file.ContentLength > limitFileSize)
                        {
                            throw new Exception($"文件{file.FileName}超出大小限制，请处理后上传！");
                        }

                        if (!string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                            savePath = filePath + "/" + fileExtension.Substring(1, fileExtension.Length - 1) + "/" +file.FileName;
                            file.SaveAs(fullPath + "/" + fileExtension.Substring(1, fileExtension.Length - 1) + "/" + file.FileName);

                            //var token = WxHelper.GetAccessToken(AppId, Secret);
                            //var mediaId = TencentHelper.UploadTempMedia(token, fullPath + "/" + fileName);
                            //var mediaId = ZXKH_UploadImgByB64(ImageToBase64(fullPath + "/" + fileName));

                            //obj.MediaId = mediaId;
                            //obj.Image = new
                            //{
                            //    media_id = mediaId
                            //};
                            obj.Content = savePath;

                            //ZXKH_SendImage(obj.ToUserName, mediaId);
                        }
                    }
                    return new Response
                    {
                        Result = WxappDao.SaveCustomerMessage(obj)  //savePath
                    };
                }
                else
                {
                    throw new Exception("上传失败，未接收到请求文件！");
                }
            }
           
        }
        /// <summary>
        /// base64 转 Image
        /// </summary>
        /// <param name="base64"></param>
        public static void Base64ToImage(string base64)
        {
            base64 = base64.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);
            Image mImage = Image.FromStream(memStream);
            Bitmap bp = new Bitmap(mImage);
            bp.Save("C:/Users/Administrator/Desktop/" + DateTime.Now.ToString("yyyyMMddHHss") + ".jpg", System.DrawingCore.Imaging.ImageFormat.Jpeg);//注意保存路径
        }

        /// <summary>
        /// Image 转成 base64
        /// </summary>
        /// <param name="fileFullName"></param>
        public static string ImageToBase64(string fileFullName)
        {
            try
            {
                Bitmap bmp = new Bitmap(fileFullName);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.DrawingCore.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length]; ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length); ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class ServiceUser
    {
        public string kf_account { get; set; }
        public string kf_headimgurl { get; set; }
        public string kf_id { get; set; }
        public string kf_nick { get; set; }
        public string kf_wx { get; set; }
    }
}