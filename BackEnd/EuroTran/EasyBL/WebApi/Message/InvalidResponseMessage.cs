namespace EasyBL.WebApi.Message
{
    public class InvalidResponseMessage : ResponseMessage
    {
        public InvalidResponseMessage(RequestMessage i_crm = null, int i_MsgCode = 500, string i_Msg = null) : base(null)
        {
            base.RESULT = ResponseResult.RR_INVALID;
            base.STATUSCODE = i_MsgCode;
            base.MSG = i_Msg;
            if (i_crm != null)
            {
                base.PROJECT = i_crm.PROJECT;
                base.PROJECTVER = i_crm.PROJECTVER;
                base.TYPE = i_crm.TYPE;
            }
        }
    }
}