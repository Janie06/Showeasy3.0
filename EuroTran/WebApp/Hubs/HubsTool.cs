using System.Collections.Generic;
using System.Configuration;
using WebApp.Models.Hub;

namespace WebApp.Hubs
{
    public class HubsTool
    {
        #region GetAppSettings

        /// <summary>
        /// 獲取WebService的配置信息
        /// </summary>
        /// <param name="sKey">appSettings中配置的Key值</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static string GetAppSettings(string sKey)
        {
            return ConfigurationManager.AppSettings[sKey].ToString();
        }

        #endregion GetAppSettings

        #region GetConnectionId

        /// <summary>
        /// 獲取人員連線id
        /// </summary>
        /// <param name="orgId">todo: describe orgId parameter on GetConnectionId</param>
        /// <param name="userId">todo: describe userId parameter on GetConnectionId</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static string GetConnectionId(string orgId, string userId)
        {
            var sConnectionId = "";
            foreach (UserInfo user in MsgHub.Euro_Online.Users)
            {
                if (userId == user.UserId && orgId == user.OrgId)
                {
                    sConnectionId = user.ConnectionId;
                    break;
                }
            }
            return sConnectionId;
        }

        #endregion GetConnectionId

        #region GetConnectionIds

        /// <summary>
        /// 獲取多人員連線id
        /// </summary>
        /// <param name="userIds">todo: describe userIds parameter on GetConnectionIds</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static IList<string> GetConnectionIds(IList<string> userIds)
        {
            var ConnectionIds = new List<string>();
            foreach (UserInfo user in MsgHub.Euro_Online.Users)
            {
                if (userIds.Contains(user.OrgId + user.UserId))
                {
                    ConnectionIds.Add(user.ConnectionId);
                }
            }
            return ConnectionIds;
        }

        #endregion GetConnectionIds

        #region GetConnectionIdsByOrgID

        /// <summary>
        /// 獲取多人員連線id
        /// </summary>
        /// <param name="OrgID">組織ID</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static IList<string> GetConnectionIdsByOrgID(string OrgID)
        {
            var ConnectionIds = new List<string>();
            foreach (UserInfo user in MsgHub.Euro_Online.Users)
            {
                if (user.OrgId == OrgID)
                {
                    ConnectionIds.Add(user.ConnectionId);
                }
            }
            return ConnectionIds;
        }

        #endregion GetConnectionIdsByOrgID
    }
}