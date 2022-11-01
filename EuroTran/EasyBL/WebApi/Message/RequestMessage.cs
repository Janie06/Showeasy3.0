using System.Collections.Generic;

namespace EasyBL.WebApi.Message
{
    /// <summary>
    /// It focus on return message.
    /// </summary>
    public class RequestMessage : MessageBase
    {
        public RequestMessage()
        {
        }

        public class StackTraceDumpItem
        {
            public string Name
            {
                get;
                set;
            }

            public List<object> Parameters
            {
                get;
                set;
            }

            public string Buttonclick
            {
                get;
                set;
            }
        }

        public class StackTraceDump
        {
            public List<RequestMessage.StackTraceDumpItem> Stack
            {
                get;
                set;
            }
        }

        public string ClientIP
        {
            get;
            set;
        }

        public RequestMessage.StackTraceDump TRACEDUMP
        {
            get;
            set;
        }

        public string LANG
        {
            get;
            set;
        }

        public string TOKEN
        {
            get;
            set;
        }

        public string SIGNATURE
        {
            get;
            set;
        }

        public string TIMESTAMP
        {
            get;
            set;
        }

        public string NONCE
        {
            get;
            set;
        }

        public Dictionary<string, string> CUSTOMDATA
        {
            get;
            set;
        }

        public string USERID
        {
            get;
            set;
        }

        public string ORIGID
        {
            get;
            set;
        }
    }
}