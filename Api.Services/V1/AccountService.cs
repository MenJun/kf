using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Api.Dao.V1;
using Api.Model.BO;
using Api.Model.DO;
using Api.Model.VO;
using Common.Authority.Core;
using Common.Utils;
using Newtonsoft.Json;
using Unity.Attributes;

namespace Api.Services.V1
{
    public class AccountService
    {
        private static readonly string ADMIN_DEFAULT_SALT = ConfigurationManager.AppSettings["admin_default_salt"];
        private static readonly string ADMIN_DEFAULT_PWD = ConfigurationManager.AppSettings["admin_default_pwd"];
        private static readonly string AppId = ConfigurationManager.AppSettings["wxAppId"];
        private static readonly string Secret = ConfigurationManager.AppSettings["wxSecret"];

        [Dependency]
        public StaffService StaffService
        {
            get;
            set;
        }

        [Dependency]
        public BaseDataDao BaseDataDao
        {
            get;
            set;
        }

        [Dependency]
        public AuthorityService AuthorityService
        {
            get;
            set;
        }


        
        [Dependency]
        public RoleDao RoleDao
        {
            get;
            set;
        }

        public Response Login(LoginVO vo)
        {
            if (vo.Phone == "administrator")
            {
                //反转字符串
                var array = vo.Noncestr.ToCharArray();
                Array.Reverse(array);
                var key = new string(array);
                // md5加密
                var md5 = new MD5CryptoServiceProvider();
                byte[] output1 = md5.ComputeHash(Encoding.Default.GetBytes(key));
                var keyStr = BitConverter.ToString(output1).Replace("-", "").ToLower();

                var ivChar = keyStr.ToCharArray();
                Array.Reverse(ivChar);
                var iv = new string(ivChar);
                byte[] output2 = md5.ComputeHash(Encoding.Default.GetBytes(iv));
                var ivStr = BitConverter.ToString(output2).Replace("-", "").ToLower().Substring(0, 16);
                //密码解密
                var password = AES256Helper.Decrypt(vo.Password, Encoding.Default.GetBytes(keyStr), Encoding.Default.GetBytes(ivStr));

                var isSuccess = BouncyCastleHashing.ValidatePassword(password, ADMIN_DEFAULT_SALT, ADMIN_DEFAULT_PWD);
                if (isSuccess)
                {
                    var token = JwtHelper.GenerateToken(-1, "administrator", 2);
                    var user = new
                    {
                        userId = -1,
                        userName = "administrator",
                        channelName = "系统管理员",
                        roleId = -1,
                        channelId = -1,
                        auth = AuthorityService.GenerateVueMenu(0, true)
                    };
                    //生成keyStr
                    var nonceStr = TimeStampHelper.ToTimeStamp(DateTime.Now) / 50 * 90;
                    byte[] output3 = md5.ComputeHash(Encoding.UTF8.GetBytes(nonceStr.ToString()));
                    var keyStr2 = BitConverter.ToString(output3).Replace("-", "").ToLower();
                    //反转keyStr 生成 ivStr
                    var ivChar2 = keyStr2.ToCharArray();
                    Array.Reverse(ivChar2);
                    var iv2 = new string(ivChar2);
                    byte[] output4 = md5.ComputeHash(Encoding.UTF8.GetBytes(iv2));
                    var ivStr2 = BitConverter.ToString(output4).Replace("-", "").ToLower().Substring(0, 16);
                    //加密
                    var payload = AES256Helper.Encrypt(JsonConvert.SerializeObject(user), Encoding.UTF8.GetBytes(keyStr2), Encoding.UTF8.GetBytes(ivStr2));
                    return new Response
                    {
                        Result = new
                        {
                            token,
                            payload,
                            noncestr = nonceStr,
                            vueRouter = AuthorityService.GenerateVueRouter(0, true)
                        }
                    };
                }
                else
                {
                    return new Response
                    {
                        Errcode = ExceptionHelper.UNKNOWN,
                        Errmsg = "密码错误！"
                    };
                }
            }
            else
            {
                dynamic staff = StaffService.QuerySystemUserByPhoneNumber(vo.Phone);
                if (staff == null)
                {
                    return new Response
                    {
                        Errcode = 10000,
                        Errmsg = "用户不存在或已被禁用"
                    };
                }
                else
                {
                    //反转字符串
                    var array = vo.Noncestr.ToCharArray();
                    Array.Reverse(array);
                    var key = new string(array);
                    // md5加密
                    var md5 = new MD5CryptoServiceProvider();
                    byte[] output1 = md5.ComputeHash(Encoding.Default.GetBytes(key));
                    var keyStr = BitConverter.ToString(output1).Replace("-", "").ToLower();

                    var ivChar = keyStr.ToCharArray();
                    Array.Reverse(ivChar);
                    var iv = new string(ivChar);
                    byte[] output2 = md5.ComputeHash(Encoding.Default.GetBytes(iv));
                    var ivStr = BitConverter.ToString(output2).Replace("-", "").ToLower().Substring(0, 16);
                    //密码解密
                    var password = AES256Helper.Decrypt(vo.Password, Encoding.Default.GetBytes(keyStr), Encoding.Default.GetBytes(ivStr));

                    var isSuccess = BouncyCastleHashing.ValidatePassword(password, (string)staff["SALT"], (string)staff["PASSWORD"]);
                    if (isSuccess)
                    {
                        var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);
                        var user = new
                        {
                            userId = (int)staff["FID"],
                            userName = (string)staff["FNAME"],
                            channelName = (string)staff["CHANNELNAME"],
                            channelCode = (string)staff["FCHANNELCODE"],
                            channelId = (int)staff["FCHANNELID"],
                            customerId = (int)staff["FCUSTOMERID"],
                            channelTypeId = (int)staff["FCHANNELTYPEID"],
                            channelTypeName = (string)staff["FCHANNELTYPENAME"],
                            roleId = (int)staff["FROLEID"],
                            modules = BaseDataDao.QueryRoleHasModules((int)staff["FROLEID"]).Select(x => x.FNAME).ToList(),
                            pers = BaseDataDao.QueryRoleHasPermissions((int)staff["FROLEID"]),
                            auth = AuthorityService.GenerateVueMenu((int)staff["FROLEID"], false)
                        };
                        //生成keyStr
                        var nonceStr = TimeStampHelper.ToTimeStamp(DateTime.Now) / 50 * 90;
                        byte[] output3 = md5.ComputeHash(Encoding.UTF8.GetBytes(nonceStr.ToString()));
                        var keyStr2 = BitConverter.ToString(output3).Replace("-", "").ToLower();
                        //反转keyStr 生成 ivStr
                        var ivChar2 = keyStr2.ToCharArray();
                        Array.Reverse(ivChar2);
                        var iv2 = new string(ivChar2);
                        byte[] output4 = md5.ComputeHash(Encoding.UTF8.GetBytes(iv2));
                        var ivStr2 = BitConverter.ToString(output4).Replace("-", "").ToLower().Substring(0, 16);
                        //加密
                        var payload = AES256Helper.Encrypt(JsonConvert.SerializeObject(user), Encoding.UTF8.GetBytes(keyStr2), Encoding.UTF8.GetBytes(ivStr2));
                        return new Response
                        {
                            Result = new
                            {
                                token,
                                payload,
                                noncestr = nonceStr,
                                vueRouter = AuthorityService.GenerateVueRouter(user.roleId, false)
                            }
                        };
                    }
                    else
                    {
                        return new Response
                        {
                            Errcode = ExceptionHelper.UNKNOWN,
                            Errmsg = "密码错误！"
                        };
                    }

                }
            }
        }

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Response RefreshToken(string token, int userId)
        {
            JwtPayload payload = JsonConvert.DeserializeObject<JwtPayload>(JwtHelper.VerifyToken(token).ToString());

            if (payload.Iss != JwtHelper.JwtIss || payload.Aud != JwtHelper.JwtAud)
            {
                throw new Exception("非法的token！");
            }
            else
            {
                var staff = StaffService.QueryStaffById(userId);
                var newToken = JwtHelper.GenerateToken(staff.FID, staff.FMOBILE, 2);
                return new Response
                {
                    Result = newToken
                };
            }
        }
        /// <summary>
        /// 生成访客token
        /// </summary>
        /// <returns></returns>
        public Response GenerateGuestToken()
        {
            var newToken = JwtHelper.GenerateToken(0, "guest", 2);
            return new Response
            {
                Result = newToken
            };
        }
        /// <summary>
        /// 初始化权限
        /// </summary>
        /// <returns></returns>
        public Response InitPermission()
        {
            AuthorityService.Initialize();
            return new Response
            {
                Result = 1
            };
        }
        /// <summary>
        /// 查询权限模块
        /// </summary>
        /// <returns></returns>
        public Response QueryRoleEnableModules(int id)
        {
            var result = AuthorityService.QueryRoleEnableAuthorityModule(id);
            return new Response
            {
                Result = result
            };
        }
        /// <summary>
        /// 更新权限模块
        /// </summary>
        /// <returns></returns>
        public Response UpdateModules(int id, int[] modules)
        {

            //BaseDataDao.DeleteRoleModulesAndPermissions(id);

            //if (modules.Length > 0)
            //{
            //    foreach (var item in modules)
            //    {
            //        BaseDataDao.InsertRoleModule(id, item);
            //    }

            //    IList<int> permissions = BaseDataDao.QueryModulesPermissions(modules);
            //    foreach (var item in permissions)
            //    {
            //        BaseDataDao.InsertRolePermission(id, item);
            //    }
            //}
            AuthorityService.UpdateRoleModule(id, modules);

            return new Response
            {
                Result = 1
            };

        }
        /// <summary>
        /// 查询权限
        /// </summary>
        /// <returns></returns>
        public Response QueryRolePermissions(int id)
        {
            //List<PermissionTree> trees = new List<PermissionTree>();
            //PermissionTree tree = new PermissionTree
            //{
            //    Title = "系统权限",
            //    Expand = true,
            //    Category = "root",
            //    Id = 0,
            //    Children = new List<PermissionTree>()
            //};
            ////查询职务下模块
            //IList<CommonBO> modules = BaseDataDao.QueryRoleHasModules(id);
            //List<int> modulesId = new List<int>();
            //foreach (var item in modules)
            //{
            //    modulesId.Add(item.FID);
            //}
            //if (modulesId.Any())
            //{
            //    return new Response
            //    {
            //        Result = trees
            //    };
            //}
            ////查询模块下页面
            //IList<CommonBO> pages = BaseDataDao.QueryPagesOfModules(modulesId.ToArray());
            //List<int> pagesId = new List<int>();
            //foreach (var item in pages)
            //{
            //    pagesId.Add(item.FID);
            //}
            //if (pagesId.Any())
            //{
            //    return new Response
            //    {
            //        Result = trees
            //    };
            //}
            ////查询页面下权限
            //IList<CommonBO> permissions = BaseDataDao.QueryPagesHasPermissionsAndRoleHasPermissions(id, pagesId.ToArray());
            //if (permissions.Any())
            //{
            //    return new Response
            //    {
            //        Result = trees
            //    };
            //}
            //// 组合数据
            //foreach (var item in modules)
            //{
            //    PermissionTree childModule = new PermissionTree
            //    {
            //        Title = item.FNAME,
            //        Expand = true,
            //        Id = item.FID,
            //        Category = "module",
            //        Children = new List<PermissionTree>()
            //    };
            //    childModule.Children = pages.Where(x => x.PARENTID == item.FID).Select(x => new PermissionTree
            //    {
            //        Title = x.FNAME,
            //        Expand = true,
            //        Id = x.FID,
            //        Category = "page",
            //        Children = permissions.Where(o => o.PARENTID == x.FID).Select(p => new PermissionTree
            //        {
            //            Title = p.FNAME,
            //            Id = p.FID,
            //            Category = "per",
            //            Checked = p.FROLEID > 0
            //        }).ToList()
            //    }).ToList();
            //    tree.Children.Add(childModule);
            //}
            //trees.Add(tree);
            var result = AuthorityService.QueryRolePermissionForTree(id);
            return new Response
            {
                Result = result
            };
        }
        /// <summary>
        /// 更新职务权限
        /// </summary>
        /// <param name="id"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public Response UpdatePermissions(int id, int[] permissions)
        {
            //BaseDataDao.DeleteRolePermissions(id);

            //foreach (var item in permissions)
            //{
            //    BaseDataDao.InsertRolePermission(id, item);
            //}
            AuthorityService.UpdateRolePermission(id, permissions);
            return new Response
            {
                Result = 1
            };
        }
        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Response RefreshToken(string token)
        {
            try
            {
                JwtPayload payload = JsonConvert.DeserializeObject<JwtPayload>(JwtHelper.VerifyToken(token).ToString());
                if (payload.Iss != JwtHelper.JwtIss || payload.Aud != JwtHelper.JwtAud)
                {
                    throw new Exception("jwt token illegal");
                }
                else
                {
                    var newToken = JwtHelper.GenerateToken(1, "Jack", 2);
                    return new Response
                    {
                        Result = newToken
                    };
                }
            }
            catch (Exception)
            {
                throw new Exception("jwt token illegal");
            }
        }

        public Response UpdatePwd(PwdVo vo)
        {
            ESSChannelStaff staff = StaffService.QueryStaffById(Convert.ToInt32(vo.UserId));
            if (staff == null)
            {
                throw new Exception("用户不存在！");
            }
            //反转字符串
            var array = vo.Noncestr.ToCharArray();
            Array.Reverse(array);
            var key = new string(array);
            // md5加密
            var md5 = new MD5CryptoServiceProvider();
            byte[] output1 = md5.ComputeHash(Encoding.Default.GetBytes(key));
            var keyStr = BitConverter.ToString(output1).Replace("-", "").ToLower();

            var ivChar = keyStr.ToCharArray();
            Array.Reverse(ivChar);
            var iv = new string(ivChar);
            byte[] output2 = md5.ComputeHash(Encoding.Default.GetBytes(iv));
            var ivStr = BitConverter.ToString(output2).Replace("-", "").ToLower().Substring(0, 16);
            //密码解密
            var password = AES256Helper.Decrypt(vo.Pwd, Encoding.Default.GetBytes(keyStr), Encoding.Default.GetBytes(ivStr));

            var isSuccess = BouncyCastleHashing.ValidatePassword(password, staff.SALT, staff.PASSWORD);
            if (isSuccess)
            {
                //生成新密码
                byte[] saltBytes = BouncyCastleHashing.CreateSalt();
                var newPwd = AES256Helper.Decrypt(vo.NewPwd, Encoding.Default.GetBytes(keyStr), Encoding.Default.GetBytes(ivStr));
                var encrypt = BouncyCastleHashing.EncryptionPassword(newPwd, saltBytes);

                staff.SALT = Convert.ToBase64String(saltBytes);
                staff.PASSWORD = encrypt;
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
                    Errmsg = "登录密码错误！"
                };
            }
        }
        /// <summary>
        /// 微信小程序用户注册
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public object WxRegister(string wxCode, ChannelStaffVO vo)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, wxCode);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errcode == null)
            {
                try
                {
                    var openid = (string)data.openid;
                    //查询手机号是否已注册
                    dynamic staff = StaffService.QueryWxappUserByPhoneNumber(vo.FMOBILE);
                    if (staff != null)
                    {
                        return new
                        {
                            Result = 0
                        };
                    }

                    vo.FWECHAT = openid;
                    vo.FENABLE = '1';
                    vo.FTELE = string.Empty;
                    vo.FCREATEDATE = System.DateTime.Now;
                    vo.FMODIFYDATE = System.DateTime.Now;
                    vo.FCREATORID = 0;
                    vo.FMODIFIERID = 0;
                    vo.FQQ = string.Empty;

                    ChannelStaffLVO staffLVO = new ChannelStaffLVO
                    {
                        FNAME = "微信注册用户",
                        FJOB = "门店客户",
                        FREMARK = string.Empty,
                        FROLEID = 3027
                    };

                    vo.ChannelStaffLVOs = staffLVO;

                    StaffService.Save(vo);
                    NHSessionProvider.GetCurrentSession().Flush();

                    dynamic temp = StaffService.QueryWxappUserByPhoneNumber(vo.FMOBILE);
                    var token = JwtHelper.GenerateToken((int)temp["FID"], (string)temp["FJOB"], 2);
                    var user = new
                    {
                        userId = (int)temp["FID"],
                        userName = (string)temp["FNAME"],
                        channelName = (string)temp["CHANNELNAME"],
                        channelCode = (string)temp["FCHANNELCODE"],
                        channelId = (int)temp["FCHANNELID"],
                        customerId = (int)temp["FCUSTOMERID"],
                        channelTypeId = (int)temp["FCHANNELTYPEID"],
                        channelTypeName = (string)temp["FCHANNELTYPENAME"],
                        modules = BaseDataDao.QueryRoleHasModules((int)temp["FROLEID"]).Select(x => x.FNAME).ToList(),
                        pers = BaseDataDao.QueryRoleHasPermissions((int)temp["FROLEID"])
                    };

                    var encryptStr = new
                    {
                        Openid = openid,
                        Desc = "NEB_DH2.2019"
                    };
                    return new
                    {
                        Result = new
                        {
                            token,
                            user
                            //id = temp["FID"],
                            ////IsOrg = 1,
                            //channelid = temp["FCHANNELID"],
                            //channelname = temp["CHANNELNAME"],
                            //Token = AES256Helper.Encrypt(JsonConvert.SerializeObject(encryptStr))
                        }
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        ErrMsg = ex.Message
                    };
                }
            }
            else
            {
                return new
                {
                    ErrMsg = (string)data.errmsg
                };
            }
        }
        /// <summary>
        /// 微信-小程序登录
        /// </summary>
        /// <param name="code">wx.login获取到的用户码</param>
        /// <returns></returns>
        public Response WxLogin(LoginVO vo)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, vo.WxCode);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg != null)
            {
                throw new Exception((string)data.errmsg);
            }
            else
            {
                return new Response
                {
                    Result = 1
                };
            }

            //else
            //{
            //    dynamic staff = StaffService.QueryByPhoneNumber(vo.Phone);
            //    if (staff == null)
            //    {
            //        //自动注册
            //        ChannelStaffVO channelStaff = new ChannelStaffVO();
            //        channelStaff.FMOBILE = vo.Phone;
            //        channelStaff.FTELE = string.Empty;
            //        channelStaff.FCREATEDATE = System.DateTime.Now;
            //        channelStaff.FMODIFYDATE = System.DateTime.Now;
            //        channelStaff.FCHANNELID = 27;
            //        channelStaff.FQQ = string.Empty;
            //        channelStaff.FWECHAT = string.Empty;
            //        channelStaff.FCREATORID = 0;
            //        channelStaff.FMODIFIERID = 0;
            //        channelStaff.FENABLE = '1';
            //        channelStaff.PASSWORD = vo.Password;
            //        ChannelStaffLVO staffLVO = new ChannelStaffLVO();
            //        channelStaff.ChannelStaffLVOs = staffLVO;
            //        staffLVO.FNAME = "微信注册用户";
            //        staffLVO.FJOB = "门店客户";
            //        staffLVO.FREMARK = string.Empty;
            //        staffLVO.FROLEID = 3027;
            //        staff = StaffService.Save(channelStaff);


            //        return new Response
            //        {
            //            Result = "注册成功"
            //        };
            //    }
            //    else
            //    {
            //        //登录

            //        //反转字符串
            //        var array = vo.Noncestr.ToCharArray();
            //        Array.Reverse(array);
            //        var key = new string(array);
            //        // md5加密
            //        var md5 = new MD5CryptoServiceProvider();
            //        byte[] output1 = md5.ComputeHash(Encoding.Default.GetBytes(key));
            //        var keyStr = BitConverter.ToString(output1).Replace("-", "").ToLower();

            //        var ivChar = keyStr.ToCharArray();
            //        Array.Reverse(ivChar);
            //        var iv = new string(ivChar);
            //        byte[] output2 = md5.ComputeHash(Encoding.Default.GetBytes(iv));
            //        var ivStr = BitConverter.ToString(output2).Replace("-", "").ToLower().Substring(0, 16);
            //        //密码解密
            //        var password = AES256Helper.Decrypt(vo.Password, Encoding.Default.GetBytes(keyStr), Encoding.Default.GetBytes(ivStr));

            //        var isSuccess = BouncyCastleHashing.ValidatePassword(password, (string)staff["SALT"], (string)staff["PASSWORD"]);
            //        if (isSuccess)
            //        {
            //            var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"]);
            //            var user = new
            //            {
            //                userId = (int)staff["FID"],
            //                userName = (string)staff["FNAME"],
            //                channelName = (string)staff["CHANNELNAME"],
            //                channelCode = (string)staff["FCHANNELCODE"],
            //                channelId = (int)staff["FCHANNELID"],
            //                customerId = (int)staff["FCUSTOMERID"],
            //                channelTypeId = (int)staff["FCHANNELTYPEID"],
            //                channelTypeName = (string)staff["FCHANNELTYPENAME"],
            //                modules = BaseDataDao.QueryRoleHasModules((int)staff["FROLEID"]).Select(x => x.FNAME).ToList(),
            //                pers = BaseDataDao.QueryRoleHasPermissions((int)staff["FROLEID"])
            //            };
            //            //生成keyStr
            //            var nonceStr = TimeStampHelper.ToTimeStamp(DateTime.Now) / 50 * 90;
            //            byte[] output3 = md5.ComputeHash(Encoding.UTF8.GetBytes(nonceStr.ToString()));
            //            var keyStr2 = BitConverter.ToString(output3).Replace("-", "").ToLower();
            //            //反转keyStr 生成 ivStr
            //            var ivChar2 = keyStr2.ToCharArray();
            //            Array.Reverse(ivChar2);
            //            var iv2 = new string(ivChar2);
            //            byte[] output4 = md5.ComputeHash(Encoding.UTF8.GetBytes(iv2));
            //            var ivStr2 = BitConverter.ToString(output4).Replace("-", "").ToLower().Substring(0, 16);
            //            //加密
            //            var payload = AES256Helper.Encrypt(JsonConvert.SerializeObject(user), Encoding.UTF8.GetBytes(keyStr2), Encoding.UTF8.GetBytes(ivStr2));
            //            return new Response
            //            {
            //                Result = new
            //                {
            //                    token,
            //                    user,
            //                    payload,
            //                    noncestr = nonceStr
            //                }
            //            };
            //        }
            //        else
            //        {
            //            return new Response
            //            {
            //                Errcode = ExceptionHelper.UNKNOWN,
            //                Errmsg = "密码错误！"
            //            };
            //        }

            //    }
            //}
        }



        /// <summary>
        /// 绑定客户-小程序
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private Response BindCustomer(WxPayloadVO payload)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, payload.Code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg == null)
            {
                var openid = (string)data.openid;
                var session_key = (string)data.session_key;
                byte[] encryData = Convert.FromBase64String(payload.EncryptedData);
                RijndaelManaged rijndaelCipher = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(session_key),
                    IV = Convert.FromBase64String(payload.Iv),
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryData, 0, encryData.Length);
                string result = Encoding.Default.GetString(plainText);
                //动态解析result 成对象
                dynamic obj = Newtonsoft.Json.Linq.JToken.Parse(result) as dynamic;

                string phoneNumber = (string)obj.phoneNumber;

                //查询手机号是否已注册
                dynamic staff = StaffService.QueryWxappUserByPhoneNumber(phoneNumber);

                if (staff == null)
                {
                    ESSChannelStaff customer = StaffService.QueryCustomerByWxopenid(openid);
                    if (customer == null)
                    {
                        ChannelStaffVO vo = new ChannelStaffVO();
                        vo.ISNEW = false;
                        vo.FCHANNELID = 27;
                        vo.FMOBILE = phoneNumber;
                        vo.XCXOPENID = openid;
                        vo.FENABLE = '1';
                        vo.FTELE = string.Empty;
                        vo.FCREATEDATE = System.DateTime.Now;
                        vo.FMODIFYDATE = System.DateTime.Now;
                        vo.FCREATORID = 0;
                        vo.FMODIFIERID = 0;
                        vo.FQQ = string.Empty;
                        vo.ISNEW = false;
                        vo.KHNAME = "用户" + UniqueKeyHelper.GetUniqueKey(8);

                        
                        ChannelStaffLVO staffLVO = new ChannelStaffLVO
                        {
                            FNAME = "微信注册用户",
                            FJOB = "客户",
                            FREMARK = string.Empty,
                            FROLEID = 1001
                        };

                        vo.ChannelStaffLVOs = staffLVO;

                        StaffService.Save(vo);

                        if (StaffService.ZXKH_Staff_Customers(openid)) //判断该用户是否有客服关联，微信小程序消息发送进行人员的第一次分配
                        {
                            StaffService.ZXKH_AddShip(openid);//添加
                            WxappService.ZXKH_WriteTxt("添加哦");
                        }


                        NHSessionProvider.GetCurrentSession().Flush();

                        customer = StaffService.QueryCustomerByWxopenid(openid);
                        ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
                        {
                            //Picture = "Image/head.png",
                            Picture = "https://zxkf.shuziyu.net//Image/newcomer.png",
                            StaffId = customer.FID,
                            UseWxAvatar = false
                        };

                        StaffService.SaveAvatar(avatar);
                    }
                    else
                    {
                        customer.FMOBILE = phoneNumber;
                    }
                    NHSessionProvider.GetCurrentSession().Flush();

                    staff = StaffService.QueryWxappUserByPhoneNumber(phoneNumber);

                    
                }
                dynamic staffXCXOpenid = StaffService.QueryWxappUserByXCXOpenid(phoneNumber);
                if (staffXCXOpenid == null)
                {
                    StaffService.QueryWxappUserEditXCXOpenid(phoneNumber,openid);
                }

                var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);

                // A3Member member = new A3Member
                // {
                //     KhName = (string)staff["FNAME"],
                //     XxAddress = "",
                //     YjAddress = "",
                //     EmployeeId = (int)staff["FID"],
                //     Phone = phoneNumber,
                //     Sex = ""
                // };

                // dynamic resp = A3Service.CheckExistsAndAddMember(token, member);
                // if ((bool)resp.isNew && !((bool)staff["ISNEW"]))
                // {
                //     StaffService.UpdateIsNewStatus((int)staff["FID"]);
                // }
                // if ((long)staff["A3ID"] == 0)
                // {
                //     StaffService.UpdateCustomerA3Id((int)staff["FID"], (long)resp.memberID);
                // }

                var user = new
                {
                    userId = (int)staff["FID"],
                    userName = (string)staff["FNAME"],
                    wxopenid = (string)staff["FWXOPENID"],
                    FJOB = (string)staff["FJOB"],
                    channelName = (string)staff["CHANNELNAME"],
                    channelCode = (string)staff["FCHANNELCODE"],
                    channelId = (int)staff["FCHANNELID"],
                    customerId = (int)staff["FCUSTOMERID"],
                    channelTypeId = (int)staff["FCHANNELTYPEID"],
                    channelTypeName = (string)staff["FCHANNELTYPENAME"],
                    avatarUrl = (string)staff["PICTURE"],
                    nickName = (string)staff["KHNAME"],
                    xcxopenid = (string)staff["XCXOPENID"],
                    phoneNumber = phoneNumber
                };

                return new Response
                {
                    Result = new
                    {
                        token,
                        user
                    }
                };

            }
            else
            {
                throw new Exception((string)data.errmsg);
            }
        }

        /// <summary>
        /// 用户是否存在
        /// </summary>
        /// <param name="staff"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        private Response User_login(dynamic staff, string phoneNumber)
        {
            var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);
            var user = new
            {
                userId = (int)staff["FID"],
                wxopenid = (string)staff["FWXOPENID"],
                FJOB = (string)staff["FJOB"],
                userName = (string)staff["FNAME"],
                channelName = (string)staff["CHANNELNAME"],
                channelCode = (string)staff["FCHANNELCODE"],
                channelId = (int)staff["FCHANNELID"],
                customerId = (int)staff["FCUSTOMERID"],
                channelTypeId = (int)staff["FCHANNELTYPEID"],
                channelTypeName = (string)staff["FCHANNELTYPENAME"],
                avatarUrl = (string)staff["PICTURE"],
                nickName = (string)staff["KHNAME"],
                phoneNumber = phoneNumber
            };

            return new Response
            {
                Result = new
                {
                    token,
                    user
                }
            };
        }

        /// <summary>
        /// 绑定客户-微信公众号进入
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private Response BindCustomerServiceStaff(WxPayloadVO payload)
        {
            //HttpClient client = new HttpClient();
            //string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            //url = string.Format(url, AppId, Secret, payload.Code);

            //HttpResponseMessage response = client.GetAsync(url).Result;
            //response.EnsureSuccessStatusCode();

            //string msg = response.Content.ReadAsStringAsync().Result;
            //dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            //if (data.errmsg == null)
            //{
            //    var openid = (string)data.openid;
            //    var session_key = (string)data.session_key;
            //    byte[] encryData = Convert.FromBase64String(payload.EncryptedData);
            //    RijndaelManaged rijndaelCipher = new RijndaelManaged
            //    {
            //        Key = Convert.FromBase64String(session_key),
            //        IV = Convert.FromBase64String(payload.Iv),
            //        Mode = CipherMode.CBC,
            //        Padding = PaddingMode.PKCS7
            //    };
            //    ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            //    byte[] plainText = transform.TransformFinalBlock(encryData, 0, encryData.Length);
            //    string result = Encoding.Default.GetString(plainText);
            //    //动态解析result 成对象
            //    dynamic obj = Newtonsoft.Json.Linq.JToken.Parse(result) as dynamic;

            //    string phoneNumber = (string)obj.phoneNumber;

            //    //查询客服角色是否存在，如不存在则添加
            //    ARole role = StaffService.QueryCustomerServiceRole();
            //    if (role == null)
            //    {
            //        role = new ARole
            //        {
            //            FNAME = "客服",
            //            FSTOREID = 27
            //        };
            //        RoleDao.Save(role);
            //    }
            //    //查询手机号是否已注册
            //    dynamic staff = StaffService.QueryCustomerServiceStaffByPhoneNumber(phoneNumber, role.FID);


            //    //2020-11-19
            //    dynamic staffYH = StaffService.QueryWxappUserByPhoneNumbers(phoneNumber);
            //    //手机号码不为空
            //    if (staffYH != null)
            //    {
            //        return User_login(staffYH, phoneNumber);//绑定客户
            //    }


            //    if (staff == null)
            //    {
            //        ChannelStaffVO vo = new ChannelStaffVO();
            //        vo.ISNEW = false;
            //        vo.FCHANNELID = 27;
            //        vo.FMOBILE = phoneNumber;
            //        vo.FWXOPENID = openid;
            //        vo.FENABLE = '1';
            //        vo.FTELE = string.Empty;
            //        vo.FCREATEDATE = System.DateTime.Now;
            //        vo.FMODIFYDATE = System.DateTime.Now;
            //        vo.FCREATORID = 0;
            //        vo.FMODIFIERID = 0;
            //        vo.FQQ = string.Empty;
            //        vo.ISNEW = false;
            //        vo.KHNAME = "客服" + UniqueKeyHelper.GetUniqueKey(8);

            //        ChannelStaffLVO staffLVO = new ChannelStaffLVO
            //        {
            //            FNAME = vo.KHNAME,
            //            FJOB = "客服",
            //            FREMARK = string.Empty,
            //            FROLEID = role.FID
            //        };

            //        vo.ChannelStaffLVOs = staffLVO;

            //        StaffService.Save(vo);

            //        NHSessionProvider.GetCurrentSession().Flush();

            //        var customerId = StaffService.QueryCustomerByWxopenid(openid, role.FID);
            //        ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
            //        {
            //            //Picture = "image/head.png",
            //            Picture = "https://zxkf.shuziyu.net//Image/newcomer.png",
            //            StaffId = Convert.ToInt32(customerId),
            //            UseWxAvatar = false
            //        };

            //        StaffService.SaveAvatar(avatar);


            //        NHSessionProvider.GetCurrentSession().Flush();

            //        staff = StaffService.QueryCustomerServiceStaffByPhoneNumber(phoneNumber, role.FID);
            //    }
            //    else
            //    {
            //        StaffService.QueryWxappUserByPhoneNumber(phoneNumber);
            //    }
            //    var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);

            //    var user = new
            //    {
            //        userId = (int)staff["FID"],
            //        wxopenid = (string)staff["FWXOPENID"],
            //        FJOB = (string)staff["FJOB"],
            //        userName = (string)staff["FNAME"],
            //        channelName = (string)staff["CHANNELNAME"],
            //        channelCode = (string)staff["FCHANNELCODE"],
            //        channelId = (int)staff["FCHANNELID"],
            //        customerId = (int)staff["FCUSTOMERID"],
            //        channelTypeId = (int)staff["FCHANNELTYPEID"],
            //        channelTypeName = (string)staff["FCHANNELTYPENAME"],
            //        avatarUrl = (string)staff["PICTURE"],
            //        nickName = (string)staff["KHNAME"],
            //        phoneNumber = phoneNumber
            //    };

            //    return new Response
            //    {
            //        Result = new
            //        {
            //            token,
            //            user
            //        }
            //    };

            //}
            //else
            //{
            //    throw new Exception((string)data.errmsg);
            //}
            

            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, payload.Code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg == null)
            {
                var openid = (string)data.openid;
                var session_key = (string)data.session_key;
                byte[] encryData = Convert.FromBase64String(payload.EncryptedData);
                RijndaelManaged rijndaelCipher = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(session_key),
                    IV = Convert.FromBase64String(payload.Iv),
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryData, 0, encryData.Length);
                string result = Encoding.Default.GetString(plainText);
                //动态解析result 成对象
                dynamic obj = Newtonsoft.Json.Linq.JToken.Parse(result) as dynamic;

                string phoneNumber = (string)obj.phoneNumber;

                //查询手机号是否已注册
                dynamic staff = StaffService.QueryWxappUserByPhoneNumber(phoneNumber);

                if (staff == null)
                {
                    ESSChannelStaff customer = StaffService.QueryCustomerByWxopenid(openid);
                    if (customer == null)
                    {

                        HttpClient clients = new HttpClient();
                        var tokens = WxappService.ZXKH_IsExistAccess_Token();
                        //HttpClient client = new HttpClient();
                        string urls = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", tokens, payload.gzhopenid);
                        HttpResponseMessage responses = client.GetAsync(urls).Result;
                        responses.EnsureSuccessStatusCode();

                        string msgs = responses.Content.ReadAsStringAsync().Result;
                        dynamic datas = JsonConvert.DeserializeObject<dynamic>(msgs);
                        // 以上openid获取用户信息

                        ChannelStaffVO vo = new ChannelStaffVO();
                        vo.ISNEW = false;
                        vo.FCHANNELID = 27;
                        vo.FMOBILE = phoneNumber;
                        vo.FWXOPENID = payload.gzhopenid;
                        vo.XCXOPENID = openid;
                        vo.FENABLE = '1';
                        vo.FTELE = string.Empty;
                        vo.FCREATEDATE = System.DateTime.Now;
                        vo.FMODIFYDATE = System.DateTime.Now;
                        vo.FCREATORID = 0;
                        vo.FMODIFIERID = 0;
                        vo.FQQ = string.Empty;
                        vo.ISNEW = false;
                        vo.KHNAME = datas.nickname;


                        ChannelStaffLVO staffLVO = new ChannelStaffLVO
                        {
                            FNAME = "微信注册用户",
                            FJOB = "客户",
                            FREMARK = string.Empty,
                            FROLEID = 1001
                        };

                        vo.ChannelStaffLVOs = staffLVO;

                        StaffService.Save(vo);

                        if (StaffService.ZXKH_Staff_Customers(openid)) //判断该用户是否有客服关联，微信小程序消息发送进行人员的第一次分配
                        {
                            StaffService.ZXKH_AddShip(openid);//添加
                            WxappService.ZXKH_WriteTxt("添加哦");
                        }


                        NHSessionProvider.GetCurrentSession().Flush();

                        customer = StaffService.QueryCustomerByWxopenid(openid);
                        ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
                        {
                            //Picture = "Image/head.png",
                            Picture = datas.headimgurl,
                            StaffId = customer.FID,
                            UseWxAvatar = false
                        };

                        StaffService.SaveAvatar(avatar);
                    }
                    else
                    {
                        customer.FMOBILE = phoneNumber;
                    }
                    NHSessionProvider.GetCurrentSession().Flush();

                    staff = StaffService.QueryWxappUserByPhoneNumber(phoneNumber);


                }
                dynamic staffFwxOpenid = StaffService.QueryWxappUserByFwxOpenid(phoneNumber);
                if (staffFwxOpenid == null)
                {
                    StaffService.QueryWxappUserEditFwxOpenid(phoneNumber, payload.gzhopenid);
                }

                var token = JwtHelper.GenerateToken((int)staff["FID"], (string)staff["FJOB"], 2);

                var user = new
                {
                    userId = (int)staff["FID"],
                    userName = (string)staff["FNAME"],
                    wxopenid = (string)staff["FWXOPENID"],
                    FJOB = (string)staff["FJOB"],
                    channelName = (string)staff["CHANNELNAME"],
                    channelCode = (string)staff["FCHANNELCODE"],
                    channelId = (int)staff["FCHANNELID"],
                    customerId = (int)staff["FCUSTOMERID"],
                    channelTypeId = (int)staff["FCHANNELTYPEID"],
                    channelTypeName = (string)staff["FCHANNELTYPENAME"],
                    avatarUrl = (string)staff["PICTURE"],
                    nickName = (string)staff["KHNAME"],
                    xcxopenid = (string)staff["XCXOPENID"],
                    phoneNumber = phoneNumber
                };

                return new Response
                {
                    Result = new
                    {
                        token,
                        user
                    }
                };

            }
            else
            {
                throw new Exception((string)data.errmsg);
            }
        }

        public Response Register(WxPayloadVO payload)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
            url = string.Format(url, AppId, Secret, payload.Code);

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string msg = response.Content.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject<dynamic>(msg);

            if (data.errmsg == null)
            {
                var openid = (string)data.openid;

                ESSChannelStaff customer = StaffService.QueryCustomerByWxopenid(openid);

                if (customer == null)
                {
                    ChannelStaffVO vo = new ChannelStaffVO();
                    vo.ISNEW = false;
                    vo.FCHANNELID = 27;
                    vo.FMOBILE = "";
                    vo.FWXOPENID = openid;
                    vo.FENABLE = '1';
                    vo.FTELE = string.Empty;
                    vo.FCREATEDATE = System.DateTime.Now;
                    vo.FMODIFYDATE = System.DateTime.Now;
                    vo.FCREATORID = 0;
                    vo.FMODIFIERID = 0;
                    vo.FQQ = string.Empty;
                    vo.KHNAME = payload.NickName;

                    ChannelStaffLVO staffLVO = new ChannelStaffLVO
                    {
                        FNAME = "微信注册用户",
                        FJOB = "门店客户",
                        FREMARK = string.Empty,
                        FROLEID = 3027
                    };

                    vo.ChannelStaffLVOs = staffLVO;

                    StaffService.Save(vo);

                    NHSessionProvider.GetCurrentSession().Flush();

                    customer = StaffService.QueryCustomerByWxopenid(openid);
                    ESSChannelStaffAvatar avatar = new ESSChannelStaffAvatar
                    {
                        Picture = payload.AvatarUrl,
                        StaffId = customer.FID,
                        UseWxAvatar = true
                    };

                    StaffService.SaveAvatar(avatar);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(customer.KHNAME))
                    {
                        customer.KHNAME = payload.NickName;
                    }
                    var avatar = StaffService.QueryCustomerAvatar(customer.FID);
                    if (avatar == null)
                    {
                        avatar = new ESSChannelStaffAvatar
                        {
                            Picture = payload.AvatarUrl,
                            StaffId = customer.FID,
                            UseWxAvatar = true
                        };

                        StaffService.SaveAvatar(avatar);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(avatar.Picture))
                        {
                            avatar.Picture = payload.AvatarUrl;
                            avatar.UseWxAvatar = true;
                        }
                    }
                }

                NHSessionProvider.GetCurrentSession().Flush();

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
        /// 微信-小程序登录
        /// </summary>
        /// <param name="code">wx.login获取到的用户码</param>
        /// <returns></returns>
        public Response BindWx(WxPayloadVO payload)
        {
            if (payload.IsCustomerServiceStaff)//微信公众号进入
            {
                return BindCustomerServiceStaff(payload);
            }
            else//小程序进入
            {
                return BindCustomer(payload);
            }
        }


    }
}


