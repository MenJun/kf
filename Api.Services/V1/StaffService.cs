using System;
using Api.Dao.V1;
using Api.Model.DO;
using Api.Model.VO;
using Common.Utils;
using Newtonsoft.Json.Linq;
using Unity.Attributes;

namespace Api.Services.V1
{
    public class StaffService
    {
        private static readonly object locker = new object();
        private static readonly string USER_DEFAULT_PWD = System.Configuration.ConfigurationManager.AppSettings["user_default_pwd"];

        [Dependency]
        public StaffDao Dao
        {
            get;
            set;
        }

        [Dependency]
        public TableMaxIdDao TableMaxIdDao
        {
            get;
            set;
        }
        
        /// <summary>
        /// 获取门店职员信息
        /// </summary>
        /// <returns></returns>
        public Response SelectStaff(string filterStr)
        {
            JObject filter = JObject.Parse(filterStr);
            if (filter["channelId"].ToString() == "-1")
            {
                filter["channelId"] = "";
            }
            var totalRecords = Dao.TotalRecords(filter);

            var roleLists = Dao.StaffList(filter);

            return new Response
            {
                Result = new
                {
                    totalRecords,
                    roleLists
                }
            };
        }
        public Response SelectStaffQuery(string filterStr)
        {
            JObject filter = JObject.Parse(filterStr);
            if (filter["channelId"].ToString() == "-1")
            {
                filter["channelId"] = "";
            }
            var totalRecords = Dao.QueryFnameTotalRecords(filter);

            var roleLists = Dao.StaffListQueryFname(filter);

            return new Response
            {
                Result = new
                {
                    totalRecords,
                    roleLists
                }
            };
        }

        /// <summary>
        /// 根据Id查询 -获取门店职员信息
        /// </summary>
        /// <returns></returns>
        public Response SelectById(int Id)
        {
            var Staffs = Dao.DetailById(Id);

            var StaffsL = Dao.DetailEntryById(Id);

            return new Response
            {
                Result = new
                {
                    Staffs,
                    StaffsL
                }
            };
        }

        public dynamic QuerySystemUserByPhoneNumber(string phoneNumber)
        {
            var staff = Dao.QuerySystemUserByPhoneNumber(phoneNumber);

            return staff;
        }

        public dynamic QueryWxappUserByPhoneNumber(string phoneNumber)
        {
            var staff = Dao.QueryWxappUserByPhoneNumber(phoneNumber);

            return staff;
        }

        public dynamic QueryWxappUserByXCXOpenid(string phoneNumber)
        {
            var staff = Dao.QueryWxappUserByXCXOpenid(phoneNumber);

            return staff;
        }

        public dynamic QueryWxappUserByFwxOpenid(string phoneNumber)
        {
            var staff = Dao.QueryWxappUserByFwxOpenid(phoneNumber);

            return staff;
        }
        public void QueryWxappUserEditXCXOpenid(string phoneNumber, string wxopenid)
        {
            Dao.QueryWxappUserEditXCXOpenid(phoneNumber,wxopenid);
        }

        /// <summary>
        /// 用户查询
        /// </summary>
        /// <param name="filter"></param>
        public Response SelectUser(JObject filter)
        {
            if (filter["channelId"].ToString() == "-1")
            {
                filter["channelId"] = "";
            }
            return Dao.SelectUser(filter);
        }

        public void QueryWxappUserEditFwxOpenid(string phoneNumber, string wxopenid)
        {
            Dao.QueryWxappUserEditFwxOpenid(phoneNumber, wxopenid);
        }

        /// <summary>
        /// 添加客户与客服的关联
        /// </summary>
        /// <param name="FWXOPENID"></param>
        public void ZXKH_AddShip(string FWXOPENID)//需要改动
        {
            var result = Dao.ZXKH_SelectStaff();
            if (result.Count != 0)               //判断管理人员是否为空
            {
                long min=(long)result[0]["Id"];
                long fpkid = (long)result[0]["FID"];
                string KfOpneid = (string)result[0]["XCXOPENID"];
                foreach (var item in result)
                {
                    if (min > item["Id"])
                    {
                        min = (long)item["Id"];
                        fpkid = (long)item["FID"];
                        KfOpneid = (string)item["XCXOPENID"];
                    }
                }
                WxappService.ZXKH_WriteTxt(min.ToString());
                WxappService.ZXKH_WriteTxt(fpkid.ToString());
                WxappService.ZXKH_WriteTxt(KfOpneid);
                //int fpkid = (int)result[result.Count - 1]["FID"];  //选取最后一个管理人fid
                //string KfOpneid = (string)result[result.Count - 1]["XCXFromOpenId"]; //选取最后一个管理人openid
                Dao.ZXKH_AddShip(FWXOPENID, fpkid);  //添加关系;
                CustomerServiceMessage WxXmlModels = new CustomerServiceMessage(); //用户默认发送消息
                WxXmlModels.MsgId = 0;
                WxXmlModels.XCXFromOpenId = FWXOPENID;
                WxXmlModels.XCXToOpenId = KfOpneid;
                WxXmlModels.MsgType = "text";
                WxXmlModels.Content = "您好，请问有时间回复吗？";
                WxXmlModels.CreateTime = WxappService.ZXKH_ConvertDateTimeInt(DateTime.Now);
                WxappService.ZXKH_savemessage(WxXmlModels);
                //var results = WxappService.ZXKH_QueryPicture(WxXmlModels.XCXFromOpenId);
                //WxXmlModels.PicUrl = (string)results["PICTURE"];
                //WxXmlModels.KHNAME = (string)results["KHNAME"];
                //var staff = WxappService.ZXKH_QueryFID(WxXmlModels.XCXToOpenId);
                //if (staff != null)
                //{
                //    var fid = staff["FID"].ToString();
                //    dynamic result1 = WxappService.ZXKH_SendWebSocket(fid);
                //    if (result1 != null)
                //    {
                //        string users = (string)result1["ConnectionID"];
                //        wxappcont ZXKH_SendWeb
                //    }
                //}
            }
            else
            {
                Dao.ZXKH_AddShip(FWXOPENID, 0);
            }
        }
        /// <summary>
        /// 判断用户是否与客服有关联
        /// </summary>
        /// <param name="FWXOPENID"></param>
        /// <returns></returns>
        public bool ZXKH_Staff_Customers(string FWXOPENID)
        {
            var result = Dao.ZXKH_Staff_Customers(FWXOPENID);
            if (result.Count == 0)
            {
                return true;
            }
            else
            {
                long staffid = result[0]["StaffID"];
                if (staffid == 0)
                {
                    Dao.ZXKH_Delete_Customers(FWXOPENID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public dynamic QueryCustomerServiceStaffByPhoneNumber(string phoneNumber, int roleId)
        {
            var staff = Dao.QueryCustomerServiceStaffByPhoneNumber(phoneNumber, roleId);

            return staff;
        }
        public dynamic QueryWxappUserByPhoneNumbers(string phoneNumber)
        {
            var staff = Dao.QueryWxappUserByPhoneNumbers(phoneNumber);

            return staff;
        }

        public ARole QueryCustomerServiceRole()
        {
            return Dao.QueryCustomerServiceRole();
        }

        /// <summary>
        /// 根据电话查询 -获取门店职员信息
        /// </summary>
        /// <param name="phoneNumber">电话号码</param>
        /// <returns></returns>
        public dynamic QueryAllUserByPhoneNumber(string phoneNumber)
        {
            var staff = Dao.QueryAllUserByPhoneNumber(phoneNumber);

            return staff;
        }
        public ESSChannelStaff QueryCustomerByWxopenid(string wxopenid)
        {
            var customer = Dao.QueryCustomerByWxopenid(wxopenid);

            return customer;
        }
        public long QueryCustomerByWxopenid(string wxopenid, int roleId)
        {
            return  Dao.QueryCustomerByWxopenid(wxopenid, roleId);
        }

        public ESSChannelStaffAvatar QueryCustomerAvatar(int id)
        {
            var avatar = Dao.QueryCustomerAvatar(id);

            return avatar;
        }

        /// <summary>
        /// 门店职员 --新增
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        public Response Save(ChannelStaffVO vo)
        {
            lock (locker)
            {
                ESSChannelStaff staff = AutoMapper.Mapper.Map<ESSChannelStaff>(vo);

                // 新增生成默认密码
                if (staff.FID == 0)
                {
                    //生成默认密码
                    byte[] saltBytes = BouncyCastleHashing.CreateSalt();
                    var encrypt = BouncyCastleHashing.EncryptionPassword(USER_DEFAULT_PWD, saltBytes);

                    staff.SALT = Convert.ToBase64String(saltBytes);
                    staff.PASSWORD = encrypt;
                }

                if (staff != null)
                {
                    staff.FID = Convert.ToInt32(TableMaxIdDao.QueryMaxId("ESS_CHANNELSTAFF"));
                    staff.FCREATEDATE = DateTime.Now.ToLocalTime();
                    staff.FMODIFYDATE = DateTime.Now.ToLocalTime();
                    Dao.Save(staff);

                    vo.ChannelStaffLVOs.FID = staff.FID;
                    // 看仔细了
                    vo.ChannelStaffLVOs.FPKID = staff.FID;
                    vo.ChannelStaffLVOs.FLOCALEID = 2052;
                    if (vo.FCHANNELID == 27 && string.IsNullOrWhiteSpace(vo.ChannelStaffLVOs.FNAME))
                    {
                        vo.ChannelStaffLVOs.FNAME = "微信注册用户";
                    }
                    EntrySave(vo.ChannelStaffLVOs);
                    return new Response
                    {
                        Result = 1
                    };
                }
                else
                {
                    return new Response
                    {
                        Errcode = ExceptionHelper.DBNOTEXISTS,
                        Errmsg = "参数不合法。",
                        Result = null
                    };
                }
            }
        }

        /// <summary>
        /// 门店职员 --编辑
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        public Response Edit(ChannelStaffVO vo)
        {
            ESSChannelStaff staff = AutoMapper.Mapper.Map<ESSChannelStaff>(vo);

            var item = Dao.Detail(staff.FID);
            //NHSessionProvider.GetCurrentSession().Merge(staff);
            //NHSessionProvider.GetCurrentSession().Clear();
            if (item != null)
            {
                item.FCHANNELID = staff.FCHANNELID;
                item.FCREATEDATE = DateTime.Now.ToLocalTime();
                item.FMODIFYDATE = DateTime.Now.ToLocalTime();
                item.FMOBILE = staff.FMOBILE;   //修改电话号码
                item.KHNAME = staff.KHNAME;    //修改用户名称
                item.AREA = staff.AREA;     //用户地区
                item.BIRTHDAY = staff.BIRTHDAY;   //用户生日
                item.FQQ = staff.FQQ;          //用户QQ
                item.FTELE = staff.FTELE;   //用户办公电话
                item.FWECHAT = staff.FWECHAT;  //用户微信
                item.GENDER = staff.GENDER;  //性别
                if (string.IsNullOrWhiteSpace(item.PASSWORD))
                {
                    //生成默认密码
                    byte[] saltBytes = BouncyCastleHashing.CreateSalt();
                    var encrypt = BouncyCastleHashing.EncryptionPassword(USER_DEFAULT_PWD, saltBytes);

                    item.SALT = Convert.ToBase64String(saltBytes);
                    item.PASSWORD = encrypt;
                }
                else
                {
                    item.SALT = item.SALT;
                    item.PASSWORD = item.PASSWORD;
                }
                Dao.Edit(item);
                ESSChannelStaff_L staffL = AutoMapper.Mapper.Map<ESSChannelStaff_L>(vo.ChannelStaffLVOs);
                staffL.FPKID = staff.FID;
                staffL.FNAME = staff.KHNAME;
                if (vo.FCHANNELID == 27)
                {
                    staffL.FNAME = "微信注册用户";
                }
                Dao.EditEntry(staffL);
                //EntryEdit(vo.ChannelStaffLVOs);
                //if (!string.IsNullOrWhiteSpace(staff.KHNAME))
                //{
                //    var data = new
                //    {
                //        memberPhone = staff.FMOBILE,
                //        name = staff.KHNAME
                //    };
                //    A3Service.UpdateCustomerInfo(data);
                //}
                //if (!string.IsNullOrWhiteSpace(staff.GENDER))
                //{
                //    var data = new
                //    {
                //        memberPhone = staff.FMOBILE,
                //        gender = staff.GENDER
                //    };
                //    A3Service.UpdateCustomerInfo(data);
                //}
                //if (!string.IsNullOrWhiteSpace(staff.BIRTHDAY))
                //{
                //    var data = new
                //    {
                //        memberPhone = staff.FMOBILE,
                //        birthday = staff.BIRTHDAY
                //    };
                //    A3Service.UpdateCustomerInfo(data);
                //}
                //if (!string.IsNullOrWhiteSpace(staff.AREA))
                //{
                //    var data = new
                //    {
                //        memberPhone = staff.FMOBILE,
                //        area = staff.AREA
                //    };
                //    A3Service.UpdateCustomerInfo(data);
                //}
                //if (!string.IsNullOrWhiteSpace(staff.KHTEL))
                //{
                //    var data = new
                //    {
                //        memberPhone = staff.FMOBILE,
                //        workPhone = staff.KHTEL
                //    };
                //    A3Service.UpdateCustomerInfo(data);
                //}
                return new Response
                {
                    Result = 1
                };
            }
            else
            {
                return new Response
                {
                    Errcode = ExceptionHelper.UNKNOWN,
                    Errmsg = "职员不存在"
                };
            }
        }

        /// <summary>
        /// 批量修改客服
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterStr"></param>
        public void DatchEditKF(int id, string filterStr)
        {
            //IList<dynamic> activicyLists1 = new List<dynamic>();
            //JObject filter = JObject.Parse(filterStr);
            string[] sArray = filterStr.Split(',');
            foreach (string FID in sArray)
            {
                Dao.DatchEditKF(id, FID);
            }
        }

        /// <summary>
        /// 门店职员副表 --新增
        /// </summary>
        /// <param name="vo"></param>
        /// <returns></returns>
        public Response EntrySave(ChannelStaffLVO vo)
        {
            ESSChannelStaff_L staffL = AutoMapper.Mapper.Map<ESSChannelStaff_L>(vo);

            if (staffL != null)
            {
                Dao.SaveEntry(staffL);
                return new Response
                {
                    Result = 1
                };

            }
            else
            {
                return new Response
                {
                    Errcode = ExceptionHelper.DBNOTEXISTS,
                    Errmsg = "参数不合法。",
                    Result = null
                };
            }
        }

        public void SaveAvatar(ESSChannelStaffAvatar avatar)
        {
            Dao.SaveAvatar(avatar);
        }
        /// <summary>
        /// 门店职员删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Response Delete(int id)
        {
            Dao.Delete(id);
            return new Response
            {
                Result = 1
            };
        }

        public ESSChannelStaff QueryStaffById(int id)
        {
            return Dao.QueryStaffById(id);
        }

        public void UpdateIsNewStatus(int staffId)
        {
            Dao.UpdateIsNewStatus(staffId);
        }

        public void UpdateCustomerA3Id(int staffId, long a3id)
        {
            Dao.UpdateCustomerA3Id(staffId, a3id);
        }
    }
}
