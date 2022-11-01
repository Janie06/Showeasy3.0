using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using EasyBL.WebApi.Message;
using Newtonsoft.Json;

namespace EasyBL
{
    public enum ActionType
    {
        帳單作廢, 取消審核, 取消過帳, 取消銷帳, 帳單刪除,
        退運帳單作廢, 取消退運審核, 取消退運過帳, 取消退運銷帳, 退運帳單刪除
    }
    /// <summary>
    /// Common 的摘要描述
    /// </summary>
    public class BillLogs
    {
        public static Tuple<bool, string> InsertBillChangeLog(SqlSugarClient db, string strLogData, ActionType ActionType, string OrgID, string UserID)
        {
            var sMsg = "";
            try
            {
                do
                {
                    var LogData = JsonConvert.DeserializeObject<OTB_OPM_BillChangeLog>(strLogData);
                    LogData.OrgID = OrgID;
                    LogData.ModifyUser = UserID;
                    LogData.ModifyDate = DateTime.Now;
                    LogData.Operation = ActionType.ToString();

                    db.Insertable(LogData).IgnoreColumns( c => c.SN).ExecuteCommand();
                } while (false);
                return new Tuple<bool, string>(true, sMsg);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                return new Tuple<bool, string>(false, sMsg);
            }

        }

    }

}