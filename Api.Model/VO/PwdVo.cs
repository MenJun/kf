using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Model.VO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/7/18 12:57:59
	*描  述  ：
	***********************************************************************/
    public class PwdVo
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserId
        {
            get;
            set;
        }
        [Required(ErrorMessage ="密码不能为空")]
        public string Pwd
        {
            get;
            set;
        }
        [Required(ErrorMessage ="新密码不能为空")]
        [MinLength(6,ErrorMessage = "密码长度不能小于6个字符")]
        public string NewPwd
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
    }
}
