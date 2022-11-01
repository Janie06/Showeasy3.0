namespace EasyBL.WebApi.Message
{
    public enum ResponseResult
    {
        RR_TRUE = 1,
        RR_FALSE = 0,
        RR_INVALID = -1
    }

    /// <summary>
    /// It focus on return message.
    /// </summary>
    public class ResponseMessage : MessageBase
    {
        public ResponseResult RESULT { get; set; }
        public int STATUSCODE { get; set; }
        public string MSG { get; set; }

        public ResponseMessage(MessageBase i_init = null)
        {
            if (null != i_init)
            {
                TYPE = i_init.TYPE;
                PROJECT = i_init.PROJECT;
                PROJECTVER = i_init.PROJECTVER;
            }
        }
    }
}