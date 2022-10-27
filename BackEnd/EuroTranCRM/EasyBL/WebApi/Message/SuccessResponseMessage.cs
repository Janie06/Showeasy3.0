namespace EasyBL.WebApi.Message
{
    public class SuccessResponseMessage : ResponseMessage
    {
        public SuccessResponseMessage(string i_sMsg, RequestMessage i_crm = null)
        {
            RESULT = ResponseResult.RR_TRUE;
            STATUSCODE = 200;
            MSG = i_sMsg;

            if (null != i_crm)
            {
                PROJECT = i_crm.PROJECT;
                PROJECTVER = i_crm.PROJECTVER;
                TYPE = i_crm.TYPE;
            }
        }
    }
}