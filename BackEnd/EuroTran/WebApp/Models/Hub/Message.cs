using System.Collections.Generic;

namespace WebApp.Models.Hub
{
    /// <summary>
    /// 消息實體模型
    /// </summary>
    public class Message
    {
        /// <summary>
        /// </summary>
        public Message()
        {
            ConnectionIds = new List<string>();

            ToUserIds = new List<string>();
        }

        /// <summary>
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// </summary>
        public string ToOrgId { get; set; }

        /// <summary>
        /// </summary>
        public string ToUserId { get; set; }

        /// <summary>
        /// </summary>
        public List<string> ConnectionIds { get; set; }

        /// <summary>
        /// </summary>
        public List<string> ToUserIds { get; set; }

        /// <summary>
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// </summary>
        public string Memo { get; set; }
    }
}