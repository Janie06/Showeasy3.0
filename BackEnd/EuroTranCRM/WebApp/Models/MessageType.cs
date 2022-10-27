namespace WebApp.Models
{
    public enum MessageType
    {
        /// <summary>
        /// 廣播
        /// </summary>
        SendBroadcast,

        /// <summary>
        /// 發送給自己
        /// </summary>
        SendToMe,

        /// <summary>
        /// 發送給連線人員
        /// </summary>
        SendToConnectionId,

        /// <summary>
        /// 發送給制定人員ID
        /// </summary>
        SendToUser,

        /// <summary>
        /// 發送給制定多個人員ID
        /// </summary>
        SendToUsers,

        /// <summary>
        /// 發送給制定群組
        /// </summary>
        SendToGroup,

        /// <summary>
        /// 進入某個群組
        /// </summary>
        JoinGroup,

        /// <summary>
        /// 推出某個群組
        /// </summary>
        LeaveGroup,

        /// <summary>
        /// 拋出異常
        /// </summary>
        Throw
    }
}