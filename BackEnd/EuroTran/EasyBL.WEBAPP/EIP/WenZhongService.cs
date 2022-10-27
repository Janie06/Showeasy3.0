using Aspose.Cells;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace EasyBL.WEBAPP.EIP
{
    public class WenZhongService : ServiceBase
    {
        #region 匯入特休假設定

        /// <summary>
        /// 匯入特休假設定
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetImport(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sFileId = _fetchString(i_crm, @"FileId");
                    var sFileName = _fetchString(i_crm, @"FileName");
                    var sRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OutFiles\Temporary\");//Word模版路徑
                    var sfileName = sFileName.Split(new string[] { @"." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    sFileName = sRoot + sFileId + @"." + sSubFileName;

                    var book = new Workbook(sFileName);
                    //book.Open(sFileName);
                    var sheet = book.Worksheets[0];
                    var cells = sheet.Cells;
                    var tbFeeItems = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, false);

                    var saWenZhong = new List<OTB_EIP_WenZhong>();
                    var saDelete = new List<string>();
                    if (tbFeeItems.Rows.Count > 0)
                    {
                        foreach (DataRow row in tbFeeItems.Rows)
                        {
                            try
                            {
                                var sWenZhongAcount = row[@"Column1"].ToString().Trim();
                                var sUserName = row[@"Column2"].ToString();
                                var sSeniority = row[@"Column6"].ToString();
                                var sEnableDate = row[@"Column7"].ToString();
                                var sExpirationDate = row[@"Column8"].ToString();
                                var sPaymentHours = row[@"Column17"].ToString();
                                var sUsedHours = row[@"Column18"].ToString();

                                var oMembers = db.Queryable<OTB_SYS_Members>()
                                    .Single(it => it.OrgID == i_crm.ORIGID && it.WenZhongAcount == sWenZhongAcount);
                                if (oMembers != null)
                                {
                                    var oWenZhong = new OTB_EIP_WenZhong
                                    {
                                        Guid = Guid.NewGuid().ToString(),
                                        OrgID = i_crm.ORIGID,
                                        UserID = oMembers.MemberID,
                                        WenZhongAcount = sWenZhongAcount,
                                        UserName = sUserName,
                                        Seniority = Convert.ToDecimal(sSeniority),
                                        EnableDate = Convert.ToDateTime(sEnableDate.Split(' ')[0]),
                                        ExpirationDate = Convert.ToDateTime(sExpirationDate.Split(' ')[0]),
                                        PaymentHours = Convert.ToDecimal(sPaymentHours),
                                        UsedHours = Convert.ToDecimal(sUsedHours),
                                        DelFlag = true
                                    };
                                    oWenZhong.RemainHours = oWenZhong.PaymentHours - oWenZhong.UsedHours;
                                    var oLSet = db.Queryable<OTB_EIP_WenZhong>()
                                        .Single(it => it.OrgID == i_crm.ORIGID && it.UserID == oMembers.MemberID && it.EnableDate == oWenZhong.EnableDate && it.ExpirationDate == oWenZhong.ExpirationDate);
                                    if (oLSet == null)
                                    {
                                        saWenZhong.Add(oWenZhong);
                                        //saDelete.Add(oLSet.Guid);
                                    }
                                }
                            }
                            catch { }
                        }
                        if (saWenZhong.Count > 0)
                        {
                            var iRel = db.Insertable<OTB_EIP_WenZhong>(saWenZhong).ExecuteCommand();
                        }
                        if (saDelete.Count > 0)
                        {
                            var iRel = db.Deleteable<OTB_EIP_WenZhong>(saDelete).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WenZhongService), @"特休假設定", @"GetImport（匯入特休假設定）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 匯入特休假設定

        #region 變更時數

        /// <summary>
        /// 變更時數
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage UpdLeaveHours(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var sUpdLeaveHours = _fetchString(i_crm, nameof(UpdLeaveHours));
                    var sMemo = _fetchString(i_crm, @"Memo");
                    var iUpdLeaveHours = Convert.ToInt32(sUpdLeaveHours);
                    var sdb = new SimpleClient<OTB_EIP_WenZhong>(db);
                    var oWenZhong = sdb.GetById(sId);
                    //增加特休是EnableDate年度為準。減少特休是ExpirationDate年度為準
                    var YearDate = oWenZhong.EnableDate.Value.Year.ToString();
                    if (iUpdLeaveHours < 0)
                        YearDate = oWenZhong.ExpirationDate.Value.Year.ToString();
                    var LeaveSet = db.Queryable<OTB_EIP_LeaveSet>()
                        .Single(it => it.OrgID == i_crm.ORIGID && it.UserID == oWenZhong.UserID && it.TYear == YearDate);
                    if (LeaveSet != null)
                    {
                        var oWenZhongUpd = new OTB_EIP_WenZhong
                        {
                            RemainHours = (oWenZhong.RemainHours ?? 0) + iUpdLeaveHours,
                            Memo = sMemo
                        };

                        #region 增加異動Log

                        var sChangeRecord = $"特休調整:{oWenZhong.EnableDate.Value.ToString("yyyy/MM/dd")}-{oWenZhong.ExpirationDate.Value.ToString("yyyy/MM/dd")}";
                        if (i_crm.LANG == @"zh")
                        {
                            sChangeRecord = ChineseStringUtility.ToSimplified(sChangeRecord);
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        Dictionary<string, string> dic = new Dictionary<string, string>
                        {
                            { "ChangeRecord", sChangeRecord },
                            { "ChangeUserName", member.MemberName },
                            { "Memo", sMemo },
                            { "ChangeHours", iUpdLeaveHours.ToString() }
                        };
                        var logInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = LeaveSet.Guid,
                            LogType = "WenZhongChange"
                        };
                        logInfo.LogInfo = JsonToString(dic);
                        _setEntityBase(logInfo, i_crm);
                        db.Insertable(logInfo).ExecuteCommand();

                        #endregion
                        var bRel = db.Updateable(oWenZhongUpd).UpdateColumns(it => new { it.Memo, it.RemainHours })
                               .Where(it => it.Guid == sId).ExecuteCommandHasChange();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, bRel);
                    }
                    else
                    {
                        sMsg = $"因為帳號{oWenZhong.UserID}的{YearDate}年度出勤設定尚未初始化，所以無法變動特休時數。" +
                            $"請先{YearDate}初始化設定再調整。";
                    }

                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WenZhongService), @"特休假設定", @"UpdLeaveHours（變更時數）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 變更時數

        #region 變更時數

        /// <summary>
        /// 變更時數
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetDel(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var sdb = new SimpleClient<OTB_EIP_WenZhong>(db);
                    var oWenZhong = sdb.GetById(sId);

                    if (!(bool)oWenZhong.DelFlag)
                    {
                        sMsg = @"該筆資料已被系統修改，暫時不可刪除";
                        break;
                    }
                    var bRel = sdb.DeleteById(oWenZhong.Guid);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WenZhongService), @"文中特休假設定", @"GetDel（刪除資料行）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 變更時數
    }
}