namespace EasyBL.WebApi.Message
{
    public class ErrorResponseMessage : ResponseMessage
    {
        public ErrorResponseMessage(string i_sMsg, RequestMessage i_crm = null) : base(null)
        {
            if (i_crm.LANG == "zh")
            {
                i_sMsg = ChineseStringUtility.ToSimplified(i_sMsg);
            }
            base.RESULT = ResponseResult.RR_FALSE;
            base.STATUSCODE = 201;
            base.MSG = i_sMsg;
            if (i_crm != null)
            {
                base.PROJECT = i_crm.PROJECT;
                base.PROJECTVER = i_crm.PROJECTVER;
                base.TYPE = i_crm.TYPE;
            }
        }
    }
}