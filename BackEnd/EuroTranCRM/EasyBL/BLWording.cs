namespace EasyBL
{
    public static class BLWording
    {
        public const string REQUEST_IS_NULL = "REQUEST, MOUDLUE OR TYPE IS NULL";
        public const string COVERT_FAIL = "CONVERT FAIL";
        public const string PASSWORD = "password";
        public const string SORT = "sort";
        public const string ID = "id";
        public const string NAME = "name";
        public const string RESULT = "result";
        public const string REL = "rel";
        public const string ROWINDEX = "RowIndex";
        public const string SERIALNUMBER = "SerialNumber";

        public const string MODIFYUSER = "ModifyUser";

        public const string FILEPATH = "filepath";

        public const string QUERYSORT = "querysort";
        public const string PAGEINDEX = "pageindex";
        public const string PAGESIZE = "pagesize";
        public const string PAGESTART = "pagestart";
        public const string PAGEEND = "pageend";

        public const string CAPTCHA = "captcha";

        public const string ISVOID = "IsVoid";

        public const string VERIFYCODEERROR = "message.VerifyCodeError";//驗證碼錯誤

        public const string VALIDATIIONCODEEXPIRED = "message.ValidationCodeExpired";//驗證碼過期，請更新後重試

        #region Email

        public const string FROMORGID = "FromUserID";     //發件人組織id
        public const string FROMUSERID = "FromUserID";     //發件人id
        public const string FROMUSERNAME = "FromUserName"; //發件人名稱
        public const string FROMEMAIL = "FromEmail";       //發件人名稱
        public const string TITLE = "Title";               //郵件標題
        public const string EMAILBODY = "EmailBody";       //郵件主體
        public const string ISCCSELF = "IsCCSelf";         //是否抄送給自己
        public const string EMAILTO = nameof(EmailTo);           //收件人集合
        public const string ATTACHMENTS = "Attachments";   //附件集合
        public const string TOUSERID = "ToUserID";         //收件人id
        public const string TOUSERNAME = "ToUserName";     //收件人名稱
        public const string ToEmail = "TOEMAIL";           //收件人email
        public const string TYPE = "Type";                 //發送類型（抄送或者密送）
        public const string MAILTEMPID = "MailTempId";     //郵件模板id
        public const string MAILDATA = "MailData";         //郵件主題資料集合

        #endregion Email
    }
}