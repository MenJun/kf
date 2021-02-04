namespace Api.Model.VO
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019-11-29 17:53:25
	*描  述  ：
	***********************************************************************/
    public class WxPayloadVO
    {
        public string EncryptedData
        {
            get;
            set;
        }
        public string Iv
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }
        public string NickName
        {
            get;
            set;
        }
        public string AvatarUrl
        {
            get;
            set;
        }
        public string gzhopenid
        {
            get;
            set;
        }
        public bool IsCustomerServiceStaff
        {
            get;
            set;
        } = false;
    }
}
