using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    public class LoginVO
    {
        [Required(ErrorMessage ="电话号码不能为空！"),MinLength(11,ErrorMessage ="电话号码不合法")]
        public string Phone
        {
            get;
            set;
        }
        [Required(ErrorMessage = "密码不能为空！"),MinLength(6,ErrorMessage ="密码不应少于6个字符")]
        public string Password
        {
            get;
            set;
        }
        [Required(ErrorMessage = "随机字符串不能为空！")]
        public string Noncestr
        {
            get;
            set;
        }

        public string WxCode
        {
            get;
            set;
        }
    }
}
